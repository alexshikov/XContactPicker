using System;

namespace XContactPicker
{
	public static class Extenstions
	{
		public static void RemoveFromSelectedContacts (this ContactsCollectionView contactsCollectionView, IContact contact)
		{
			var index = contactsCollectionView.SelectedContacts.IndexOf (contact);
			if (index >= 0) {
				contactsCollectionView.RemoveFromSelectedContacts (index);
			}
		}
	}
}

