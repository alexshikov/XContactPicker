using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Linq;

namespace XContactPicker.Sample
{
	public class SampleViewController: UIViewController
	{
		private ContactsCollectionView header;
		private UITableView tableView;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var contacts = new List<Contact>
			{
				new Contact ("Eddard", "ed@winterfell.com"),
				new Contact ("Jon", "jon@winterfell.com"),
				new Contact ("Jaime", "jaime@lannister.com"),
				new Contact ("Cercei", "cercei@lannister.com"),
			};

			header = new ContactsCollectionView (new RectangleF (0, 20, 320, 30), 30);
			header.BackgroundColor = UIColor.FromRGB (245, 245, 245);
			header.ContentSizeChanged += UpdateSize;
			header.ContactRemoved += (c) => {
				var index = contacts.IndexOf ((Contact)c);
				if (index >= 0) {
					tableView.DeselectRow (NSIndexPath.FromRowSection (index, 0), true);
				}
			};

			var source = new TableSource (contacts);
			source.ContactSelected += (c) => {
				header.AddToSelectedContacts (c);
			};
			source.ContactDeselected += (c) => {
				var index = header.SelectedContacts.IndexOf (c);
				if (index >= 0) {
					header.RemoveFromSelectedContacts (index);
				}
			};

			tableView = new UITableView (View.Bounds);
			tableView.AllowsMultipleSelection = true;
			tableView.Source = source;

			View.AddSubviews (tableView, header);

			header.ReloadData ();
			UpdateSize (header.Frame.Size);
		}

		private void UpdateSize (SizeF size)
		{
			var frame = header.Frame;
			frame.Height = Math.Min (64, size.Height);
			header.Frame = frame;

			var inset = new UIEdgeInsets (frame.Bottom, 0, 0, 0);
			tableView.ContentInset = inset;
			tableView.ScrollIndicatorInsets = inset;
		}

		#region TableSource

		private class TableSource: UITableViewSource
		{
			private static readonly NSString identifier = new NSString ("cell");

			private readonly IList<Contact> contacts;

			public event Action<Contact> ContactSelected = delegate {};
			public event Action<Contact> ContactDeselected = delegate {};

			public TableSource (IList<Contact> contacts)
			{
				this.contacts = contacts;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (identifier) 
										?? new UITableViewCell (UITableViewCellStyle.Subtitle, identifier);

				var contact = contacts [indexPath.Row];
				cell.TextLabel.Text = contact.Title;
				cell.DetailTextLabel.Text = contact.Subtitle;
				return cell;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return contacts.Count;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				ContactSelected (contacts [indexPath.Row]);
			}

			public override void RowDeselected (UITableView tableView, NSIndexPath indexPath)
			{
				ContactDeselected (contacts [indexPath.Row]);
			}
		}

		#endregion
	}
}

