using System;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		NavigationManager? NavigationManager => Handler.MauiContext?.GetNavigationManager();

		UINavigationController? NavigationController => NavigationManager?.NavigationController;

		public static void MapIsVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			if (toolbar.NavigationController == null)
			{
				throw new NullReferenceException("NavigationController is null.");
			}
			toolbar.NavigationController?.SetNavigationBarHidden(!toolbar.IsVisible, false);
		}
	}
}
