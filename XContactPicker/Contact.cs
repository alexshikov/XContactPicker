using System;

namespace XContactPicker
{
	public class Contact: IContact
	{
		public string Title { get; set; }
		public string Subtitle { get; set; }

		public Contact (string title, string subtile)
		{
			Title = title;
			Subtitle = subtile;
		}
	}
}

