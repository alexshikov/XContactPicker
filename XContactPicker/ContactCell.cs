using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace XContactPicker
{
	public class ContactCell: UICollectionViewCell
	{
		private IContact contact;
		public IContact Contact 
		{ 
			get { return contact; }
			set 
			{ 
				contact = value;
				label.Text = contact.Title + ",";
			} 
		}

		private bool isFocused;
		public bool IsFocused 
		{ 
			get { return isFocused; }
			set
			{
				isFocused = value;
				Update ();
			}
		}

		private UILabel label;

		public ContactCell (CGRect frame)
			: base (frame)
		{
			Setup ();
		}

		public ContactCell ()
		{
			Setup ();
		}

		public ContactCell (IntPtr handle)
			: base (handle)
		{
			Setup ();
		}

		public nfloat WidthForContact (IContact contact)
		{
			var rect = new NSString (contact.Title ?? string.Empty)
							.GetBoundingRect (new CGSize (float.MaxValue, float.MaxValue), 
												(NSStringDrawingOptions)0,
												new UIStringAttributes { Font = label.Font }, 
												null);
			return rect.Width + 10;
		}

		private void Setup ()
		{
			#if DEBUG_BORDERS
			Layer.BorderColor = UIColor.Orange.CGColor;
			Layer.BorderWidth = 1.0f;
			#endif

			var labelStyle = UILabel.AppearanceWhenContainedIn (GetType ());

			label = new UILabel (Bounds) 
			{
				TextColor = TintColor,
				TextAlignment = UITextAlignment.Center,
				ClipsToBounds = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			if (labelStyle.Font != null) {
				label.Font = labelStyle.Font;
			}

			AddSubview (label);

			AddConstraints (NSLayoutConstraint.FromVisualFormat ("H:|-(2)-[label]-(2)-|", 
								(NSLayoutFormatOptions) 0, "label", label));
			AddConstraints (NSLayoutConstraint.FromVisualFormat ("V:|[label]|", 
								(NSLayoutFormatOptions) 0, "label", label));
		}

		private void Update ()
		{
			if (isFocused)
			{
				label.TextColor = UIColor.White;
				label.BackgroundColor = TintColor;
				label.Layer.CornerRadius = 3.0f;
			}
			else
			{
				label.TextColor = TintColor;
				label.BackgroundColor = UIColor.Clear;
			}
		}

		public override void TintColorDidChange ()
		{
			base.TintColorDidChange ();
			Update ();
		}
	}
}

