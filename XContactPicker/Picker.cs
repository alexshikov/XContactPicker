using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace XContactPicker
{
	public class Picker: UIView
	{
		private ContactsCollectionView entryView;

		public Picker (RectangleF frame)
			: base (frame)
		{
			Setup ();
		}

		private void Setup ()
		{
			entryView = new ContactsCollectionView (Bounds, 30);

#if DEBUG_BORDERS
			Layer.BorderColor = UIColor.Gray.CGColor;
		    Layer.BorderWidth = 1.0f;
			entryView.Layer.BorderColor = UIColor.Red.CGColor;
			entryView.Layer.BorderWidth = 1.0f;
//		    searchTableView.layer.borderColor = [UIColor blueColor].CGColor;
//		    searchTableView.layer.borderWidth = 1.0;
#endif

			AddSubview (entryView);
		}

		public void Reload ()
		{
			entryView.ReloadData ();
		}
	}
}

