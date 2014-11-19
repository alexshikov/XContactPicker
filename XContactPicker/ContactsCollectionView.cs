using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using System.Collections;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;

namespace XContactPicker
{
	[Adopts ("UIKeyInput")]
	public class ContactsCollectionView: UICollectionView
	{
		public readonly float CellHeight;

		public event Action<IContact> ContactAdded = delegate {};
		public event Action<IContact> ContactRemoved = delegate {};
		public event Action<IContact> ContactSelected = delegate {};
		public event Action<string> EntryTextChanged = delegate {};
		public event Action<SizeF> ContentSizeChanged = delegate {};

		public string SearchText { get; set; }
		public string PromptText { get; set; }

		public bool AllowsTextInput { get; set; }
		public bool ShowPrompt { get; set; }

		public override RectangleF Frame 
		{
			set 
			{
				if (base.Frame == value) {
					return;
				}

				if (CollectionViewLayout != null) {
					CollectionViewLayout.InvalidateLayout ();
				}

				base.Frame = value;

				if (CollectionViewLayout != null) {
					((CollectionFlowLayout)CollectionViewLayout).FinalizeCollectionViewUpdates ();
				}
			}
		}

		public override RectangleF Bounds 
		{
			set 
			{
				if (base.Bounds == value) {
					return;
				}

				if (CollectionViewLayout != null) {
					CollectionViewLayout.InvalidateLayout ();
				}

				base.Bounds = value;

				if (CollectionViewLayout != null) {
					((CollectionFlowLayout)CollectionViewLayout).FinalizeCollectionViewUpdates ();
				}
			}
		}

		// Important to return YES here if we want to become the first responder after a child (i.e., entry UITextField)
		// has given it up so we can respond to keyboard events
		public override bool CanBecomeFirstResponder 
		{
			get { return DateTime.UtcNow > preventResponseTime; }
		}

		private float MaxContentWidth
		{
			get 
			{ 
				var inset = ((UICollectionViewFlowLayout)CollectionViewLayout).SectionInset;
				return Frame.Width - inset.Left - inset.Right;
			}
		}

		public readonly IList<IContact> SelectedContacts = new List<IContact> ();

		public ContactsCollectionView (RectangleF frame, float cellHeight)
			: base (frame, new CollectionFlowLayout ())
		{
			CellHeight = cellHeight;
			Setup ();
		}

		private void Setup ()
		{
			#if DEBUG_BORDERS
			Layer.BorderColor = UIColor.Cyan.CGColor;
			Layer.BorderWidth = 1.0f;
			#endif

			var layout = (CollectionFlowLayout)CollectionViewLayout;
			layout.MinimumInteritemSpacing = 5;
			layout.MinimumLineSpacing = 1;
			layout.SectionInset = new UIEdgeInsets (0, 10, 0, 10);

			PromptText = "To:";
			SearchText = " ";

			AllowsSelection = true;
			AllowsMultipleSelection = false;
			BackgroundColor = UIColor.White;

			AllowsTextInput = true;
			ShowPrompt = true;

			RegisterClassForCell (typeof(ContactCell), new NSString (typeof(ContactCell).Name));
			RegisterClassForCell (typeof(EntryCell), new NSString (typeof(EntryCell).Name));
			RegisterClassForCell (typeof(PromptCell), new NSString (typeof(PromptCell).Name));

			Source = new CollectionDataSource (this);
		}

		// NOTE there is a case when the keyboard is hidden by some another event
		// in that case iOS asks ContactsCollectionView to become a responder instead
		// and so that the keyboard appears again
		// this method allows to set a delay to prevent become a responder for some short time 
		// (approximatelly the time of keyboard hide animation plus few hundreds of additional millis)
		private DateTime preventResponseTime;
		public void PreventBecomeResponder (TimeSpan timeSpan)
		{
			preventResponseTime = DateTime.UtcNow + timeSpan;
		}

		public override bool ResignFirstResponder ()
		{
			var selectedItems = GetIndexPathsForSelectedItems ();
			if (selectedItems.Length > 0)
			{
				foreach (var indexPath in selectedItems)
				{
					DeselectItem (indexPath, true);
					Source.ItemDeselected (this, indexPath);
				}

			}

			RemoveFocusFromEntry ();

			base.ResignFirstResponder ();

			return true;
		}

		public void AddToSelectedContacts (IContact contact, Action onCompleted = null)
		{
			if (IndexPathsForVisibleItems.Contains (EntryCellIndexPath))
			{
				var entryCell = CellForItem (EntryCellIndexPath) as EntryCell;
				if (entryCell != null) {
					entryCell.Reset ();
				}
			}
			else
			{
				SearchText = " ";
			}

			if (!SelectedContacts.Contains (contact))
			{
				SelectedContacts.Add (contact);
				var offset = ContentOffset;
				PerformBatchUpdates (() => {

					InsertItems (new [] { NSIndexPath.FromRowSection (SelectedContacts.Count - (ShowPrompt ? 0 : 1), 0) });
					ContentOffset = offset;
				
				}, _ => {

					if (onCompleted != null) {
						onCompleted ();
					}

					ContactAdded (contact);

				});

			}
		}

		public void RemoveFromSelectedContacts (int index, Action onComplete = null)
		{
			var selectedItems = GetIndexPathsForSelectedItems ();
			if (SelectedContacts.Count + 1 > selectedItems.Length && index >= 0 && index < SelectedContacts.Count)
			{
				var contact = SelectedContacts [index];
				PerformBatchUpdates (() => {

					SelectedContacts.RemoveAt (index);
					var selectedCell = IndexPathOfSelectedCell;
					if (selectedCell != null) {
						DeselectItem (selectedCell, false);
					}
					DeleteItems (new [] { NSIndexPath.FromRowSection (index + (ShowPrompt ? 1 : 0), 0) });
					ScrollToItem (EntryCellIndexPath, UICollectionViewScrollPosition.None, true);

				}, (b) => {

					if (onComplete != null) {
						onComplete ();
					}

					ContactRemoved (contact);

					SetFocusOnEntry ();

				});

			}
		}

		#region Helper methods

		public int EntryCellIndex 
		{ 
			get { return SelectedContacts.Count + (ShowPrompt ? 1 : 0); } 
		}

		public NSIndexPath EntryCellIndexPath
		{ 
			get { return NSIndexPath.FromRowSection (EntryCellIndex, 0); } 
		}

		private bool IsEntryCell (NSIndexPath indexPath)
		{
			return indexPath.Row == EntryCellIndex;
		}

		private bool IsPromptCell (NSIndexPath indexPath)
		{
			return ShowPrompt && indexPath.Row == 0;
		}

		private bool IsContactCell (NSIndexPath indexPath)
		{
			return !IsEntryCell (indexPath) && !IsPromptCell (indexPath);
		}

		private int SelectedContactIndex (NSIndexPath indexPath)
		{
			return SelectedContactIndex (indexPath.Row);
		}

		private int SelectedContactIndex (int row)
		{
			return row - (ShowPrompt ? 1 : 0);
		}

		private NSIndexPath IndexPathOfSelectedCell
		{
			get { return GetIndexPathsForSelectedItems ().FirstOrDefault (); }
		}

		private void SetFocusOnEntry ()
		{
			var action = new Action (() => {

				var cell = CellForItem (EntryCellIndexPath) as EntryCell;
				if (cell != null) {
					cell.SetFocus ();
				}

			});

			if (IsEntryVisible) 
			{
				action ();
			}
			else
			{
				ScrollToEntryAnimated (true, action);
			}
		}

		private void RemoveFocusFromEntry ()
		{
			var cell = CellForItem(EntryCellIndexPath) as EntryCell;
			if (cell != null) {
				cell.RemoveFocus ();
			}
		}

		private bool IsEntryVisible
		{
			get { return IndexPathsForVisibleItems.Contains (EntryCellIndexPath); }
		}

		private void ScrollToEntryAnimated (bool animated, Action onComplete)
		{
			if (animated)
			{
				UIView.Animate (0.25, () => {
					ContentOffset = new PointF (0, ContentSize.Height - Bounds.Height);
				}, new NSAction(onComplete));
			}
			else if (ShowPrompt)
			{
				ScrollToItem (EntryCellIndexPath, UICollectionViewScrollPosition.Bottom, false);
			}
		}

		private void EntryCellTextChanged (string text)
		{
			SearchText = text;
			EntryTextChanged (text);
		}

		private void EntryCellBackspaceDetected ()
		{
			if (SelectedContacts.Count > 0)
			{
				//[textField resignFirstResponder];

				var selectedIndexPath = NSIndexPath.FromRowSection (SelectedContacts.Count - (ShowPrompt ? 0 : 1), 0);
				SelectItem (selectedIndexPath, true, UICollectionViewScrollPosition.Bottom);

				Source.ItemSelected (this, selectedIndexPath);
				BecomeFirstResponder ();
			}
		}

		#endregion

		private SizeF latestSize;
		internal void RaiseContentSizeChanged (SizeF size)
		{
			if (latestSize == size) {
				return;
			}
			ContentSizeChanged (size);
		}

		#region UIKeyInput

		[Export ("deleteBackward")]
		void DeleteBackward ()
		{
			var selectedItems = GetIndexPathsForSelectedItems ();
			if (selectedItems.Length > 0)
			{
				RemoveFromSelectedContacts (SelectedContactIndex (IndexPathOfSelectedCell.Row), null);
			}
		}

		[Export ("hasText")]
		bool HasText
		{
			get { return true; }
		}

		[Export ("insertText:")]
		void InsertText (string text)
		{
		}

		[Export ("autocorrectionType")]
		UITextAutocorrectionType AutocorrectionType
		{
			get { return UITextAutocorrectionType.No; }
		}

		#endregion

		#region CollectionDataSource

		private class CollectionDataSource: UICollectionViewSource, IUICollectionViewDelegateFlowLayout
		{
			private readonly ContactsCollectionView cv;

			public CollectionDataSource (ContactsCollectionView cv)
			{
				this.cv = cv;
			}

			public override int NumberOfSections (UICollectionView collectionView)
			{
				return 1;
			}

			public override int GetItemsCount (UICollectionView collectionView, int section)
			{
				return cv.SelectedContacts.Count + (cv.ShowPrompt ? 1 : 0) + 1;
			}

			public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
			{
				UICollectionViewCell resultCell;

				if (cv.IsPromptCell (indexPath))
				{
					var id = new NSString (typeof(PromptCell).Name);
					var cell = (PromptCell)collectionView.DequeueReusableCell (id, indexPath);
					cell.Text = cv.PromptText;

					resultCell = cell;
				}
				else if (cv.IsEntryCell (indexPath))
				{
					var id = new NSString (typeof(EntryCell).Name);
					var cell = (EntryCell)collectionView.DequeueReusableCell (id, indexPath);

					cell.Text = cv.SearchText;
					cell.IsEnabled = cv.AllowsTextInput;

					cell.TextChanged += cv.EntryCellTextChanged;
					cell.BackspaceDetected += cv.EntryCellBackspaceDetected;

					if (cv.IsFirstResponder && cv.IndexPathOfSelectedCell == null)
					{
						cell.SetFocus ();
					}

					resultCell = cell;
				}
				else
				{
					var id = new NSString (typeof(ContactCell).Name);
					var cell = (ContactCell)collectionView.DequeueReusableCell (id, indexPath);
					cell.Contact = cv.SelectedContacts [cv.SelectedContactIndex (indexPath)];
					cell.IsFocused = (cv.IndexPathOfSelectedCell == indexPath);

					resultCell = cell;
				}

				return resultCell;
			}

			public override void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath)
			{
				var cell = (ContactCell)collectionView.CellForItem (indexPath);
				cv.BecomeFirstResponder ();
				if (cell != null) 
				{
					cell.IsFocused = true;
					cv.ContactSelected (cell.Contact);
				}
			}

			public override bool ShouldHighlightItem (UICollectionView collectionView, NSIndexPath indexPath)
			{
				return cv.IsContactCell (indexPath);
			}

			public override void ItemDeselected (UICollectionView collectionView, NSIndexPath indexPath)
			{
				var cell = (ContactCell)collectionView.CellForItem (indexPath);
				cell.IsFocused = false;
			}

			private PromptCell promptPrototypeCell;
			private EntryCell entryPrototypeCell;
			private ContactCell contactPrototypeCell;

			[Export("collectionView:layout:sizeForItemAtIndexPath:")]
			public SizeF GetSizeForItem (UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
			{
				float width;

				if (cv.IsPromptCell (indexPath))
				{
					if (promptPrototypeCell == null) {
						promptPrototypeCell = new PromptCell ();
					}
					width = promptPrototypeCell.WidthForText (cv.PromptText);
					width += 20;
				}
				else if (cv.IsEntryCell (indexPath))
				{
					if (entryPrototypeCell == null) {
						entryPrototypeCell = new EntryCell ();
					}
					width = Math.Max (50, entryPrototypeCell.WidthForText (cv.SearchText));
				}
				else
				{
					if (contactPrototypeCell == null) {
						contactPrototypeCell = new ContactCell ();
					}

					var contact = cv.SelectedContacts [cv.SelectedContactIndex (indexPath)];
					width = contactPrototypeCell.WidthForContact (contact);
				}

				return new SizeF (Math.Min (cv.MaxContentWidth, width), cv.CellHeight);
			}
		}

		#endregion
	}
}

