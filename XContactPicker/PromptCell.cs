using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace XContactPicker
{
	public class PromptCell: UICollectionViewCell
	{
		private string text;
		public string Text 
		{ 
			get { return text; }
			set 
			{ 
				text = value; 
				if (label != null) {
					label.Text = value;
				}
			}
		}

		public UIEdgeInsets Insets { get; set; }

		private UILabel label;

		public PromptCell ()
		{
			Setup ();
		}

		public PromptCell (CGRect frame)
			: base (frame)
		{
			Setup ();
		}

		public PromptCell (IntPtr handle)
			: base (handle)
		{
			Setup ();
		}

		public nfloat WidthForText (string text)
		{
			var rect = new NSString (text).GetBoundingRect (new CGSize (float.MaxValue, float.MaxValue), 
												NSStringDrawingOptions.UsesLineFragmentOrigin,
												new UIStringAttributes { Font = label.Font }, 
												null);
			return rect.Width;
		}

		private void Setup ()
		{
			Insets = new UIEdgeInsets (0, 5, 0, 5);
#if DEBUG_BORDERS
			Layer.BorderWidth = 1f;
			Layer.BorderColor = UIColor.Purple.CGColor;
#endif
			var labelStyle = UILabel.AppearanceWhenContainedIn (GetType ());

			label = new UILabel () 
			{
				TranslatesAutoresizingMaskIntoConstraints = false,
				TextAlignment = UITextAlignment.Center,
				TextColor = labelStyle.TextColor,
			};

			if (labelStyle.Font != null) {
				label.Font = labelStyle.Font;
			}

			AddSubview (label);

			AddConstraints (NSLayoutConstraint.FromVisualFormat ("H:|[label]|", 
								(NSLayoutFormatOptions) 0, "label", label));
			AddConstraints (NSLayoutConstraint.FromVisualFormat ("V:|[label]|", 
								(NSLayoutFormatOptions) 0, "label", label));
		}
	}
}

