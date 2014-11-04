using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace XContactPicker
{
	public class CollectionFlowLayout: UICollectionViewFlowLayout
	{
		public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect (RectangleF rect)
		{
			var attributes = base.LayoutAttributesForElementsInRect(rect);

			foreach (var attribute in attributes)
			{
				if (attribute.RepresentedElementKind == null)
				{
					var indexPath = attribute.IndexPath;
					attribute.Frame = LayoutAttributesForItem (indexPath).Frame;
				}
			}

			return attributes;
		}

		public override UICollectionViewLayoutAttributes LayoutAttributesForItem (NSIndexPath indexPath)
		{
			RectangleF frame;

			var currentItemAttributes = base.LayoutAttributesForItem (indexPath);
			var sectionInset = ((UICollectionViewFlowLayout)CollectionView.CollectionViewLayout).SectionInset;

			var total = CollectionView.Source.GetItemsCount (CollectionView, 0);

			if (indexPath.Item == 0)
			{
				// first item of section
				frame = currentItemAttributes.Frame;
				// first item of the section should always be left aligned
				frame.X = sectionInset.Left;
				currentItemAttributes.Frame = frame;

				return currentItemAttributes;
			}

			var previousIndexPath = NSIndexPath.FromItemSection (indexPath.Item - 1, indexPath.Section);
			var previousFrame = base.LayoutAttributesForItem(previousIndexPath).Frame;

			var currentFrame = currentItemAttributes.Frame;
			var stretchedCurrentFrame = new RectangleF (0, currentFrame.Y, CollectionView.Frame.Width, currentFrame.Height);

			if (!previousFrame.IntersectsWith(stretchedCurrentFrame))
			{
				// if current item is the first item on the line
				// the approach here is to take the current frame, left align it to the edge of the view
				// then stretch it the width of the collection view, if it intersects with the previous frame then that means it
				// is on the same line, otherwise it is on it's own new line
				frame = currentItemAttributes.Frame;
				frame.X = sectionInset.Left; // first item on the line should always be left aligned
				if (indexPath.Row == total - 1)
				{
					var newWidth = CollectionView.Frame.Width - sectionInset.Left - sectionInset.Right;
					frame.Width = Math.Max(Math.Max(50, newWidth), frame.Width);
				}
				currentItemAttributes.Frame = frame;
				return currentItemAttributes;
			}

			frame = currentItemAttributes.Frame;
			frame.X = previousFrame.Right;
			if (indexPath.Row == total - 1)
			{
				var newWidth = CollectionView.Frame.Width - previousFrame.Right - sectionInset.Right;
				frame.Width = Math.Max(Math.Max(50, newWidth), frame.Width);
			}
			currentItemAttributes.Frame = frame;
			return currentItemAttributes;
		}

		public override bool ShouldInvalidateLayoutForBoundsChange (RectangleF newBounds)
		{
			return true;
		}

		public override void FinalizeCollectionViewUpdates ()
		{
			var contactsCollectionView = CollectionView as ContactsCollectionView;
			if (contactsCollectionView != null)
			{
				contactsCollectionView.RaiseContentSizeChanged (CollectionViewContentSize);
			}
		}
	}
}

