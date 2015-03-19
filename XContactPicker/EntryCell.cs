using System;
using UIKit;
using CoreGraphics;
using Foundation;

namespace XContactPicker
{
	public class EntryCell: UICollectionViewCell
	{		
		public event Action<string> TextChanged = delegate {};
		public event Action BackspaceDetected = delegate {};
				 
		private const string WhiteSpace = " ";

		public bool IsEnabled { get; set; }

		private UITextField textField;
		private object notificationToken;
		private string placeholderText;

		public EntryCell ()
		{
			Setup ();
		}

		public EntryCell (CGRect frame)
			: base (frame)
		{
			Setup ();
		}

		public EntryCell (IntPtr handle)
			: base (handle)
		{
			Setup ();
		}

		public void SetFocus ()
		{
			textField.BecomeFirstResponder ();
		}

		public void RemoveFocus ()
		{
			textField.ResignFirstResponder ();
		}

		public void Reset ()
		{
			textField.Text = WhiteSpace;
			TextChanged (textField.Text);
		}

		public nfloat WidthForText (string text)
		{
			var rect = new NSString (text).GetBoundingRect (new CGSize (float.MaxValue, float.MaxValue), 
												NSStringDrawingOptions.UsesLineFragmentOrigin,
												new UIStringAttributes { Font = textField.Font }, 
												null);
			return rect.Width;
		}

		private void Setup ()
		{
			var labelStyle = UILabel.AppearanceWhenContainedIn (GetType ());

			textField = new UITextField (Bounds)
			{
				AutocorrectionType = UITextAutocorrectionType.No,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			if (labelStyle.Font != null) {
				textField.Font = labelStyle.Font;
			}

			textField.ShouldReturn = delegate { return false; };
			textField.EditingDidBegin += delegate { RemovePlaceholder(); };
			textField.ShouldChangeCharacters = ShouldChangeCharacters;

#if DEBUG_BORDERS
			Layer.BorderColor = UIColor.Orange.CGColor;
			Layer.BorderWidth = 1.0f;
			textField.Layer.BorderColor = UIColor.Green.CGColor;
			textField.Layer.BorderWidth = 2.0f;
#endif

			AddSubview (textField);

			AddConstraints (NSLayoutConstraint.FromVisualFormat ("H:|[textField]|", 
								(NSLayoutFormatOptions) 0, "textField", textField));
			AddConstraints (NSLayoutConstraint.FromVisualFormat ("V:|[textField]|", 
								(NSLayoutFormatOptions) 0, "textField", textField));

			// setup text field change
			notificationToken = UITextField.Notifications.ObserveTextFieldTextDidChange (GlobalTextChanged);
			notificationToken.ToString (); // to avoid warning
		}

		private void GlobalTextChanged (object sender, NSNotificationEventArgs args)
		{
			if (args.Notification.Object == textField) 
			{
				TextChanged (textField.Text);
			}
		}

		public void SetPlaceholder(string text)
		{	
			placeholderText = text;
			if (textField.Text != text)
			{
				textField.Text = text;
				textField.TextColor = UIColor.LightGray;
			}
		}

		private void RemovePlaceholder()
		{
			if (textField.Text == placeholderText)
			{
				textField.Text = WhiteSpace;
				textField.TextColor = UIColor.Black;
			}
		}

		private bool ShouldChangeCharacters (UITextField tf, NSRange range, string replacement)
		{
			var newString = new NSString (tf.Text).Replace (range, new NSString (replacement)).ToString ();

			// If backspace is pressed and there isn't any text in the field, we want to select the
			// last selected contact and not let them delete the space we inserted (the space allows
			// us to catch the last backspace press - without it, we get no event!)
			if (string.IsNullOrEmpty (newString) && 
				string.IsNullOrEmpty (replacement) && 
				range.Location == 0 && 
				range.Length == 1)
			{
				BackspaceDetected ();
				return false;
			}

			return true;
		}
	}
}

