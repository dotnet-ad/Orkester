using System;

using Xamarin.Forms;

namespace Orkester
{
	public class Orkester : ContentPage
	{
		public Orkester()
		{
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Hello ContentPage" }
				}
			};
		}
	}
}


