using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, UINavigationController>
	{
		NavigationManager? NavigationManager => MauiContext?.GetNavigationManager();

		public UINavigationController NavigationController => NavigationManager?.NavigationController ?? throw new NullReferenceException("Could not obtain NavigationController.");

		protected override UINavigationController CreatePlatformElement()
		{
			return NavigationManager?.NavigationController ?? throw new NullReferenceException("Could not obtain Navigation controller.");
		}

		// TODO: Check if this happens for all toolbars, not just NavigationPageToolbar
		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			if (arg1 is ToolbarHandler toolbarHandler)
			{
				toolbarHandler.PlatformView.TopViewController?.UpdateNavigationBarTitle(arg2.Title);
			}
		}

		public static void MapIsVisible(IToolbarHandler handler, IToolbar toolbar)
		{
			if (handler is ToolbarHandler toolbarHandler)
			{
				toolbarHandler.PlatformView.UpdateNavigationBarVisibility(toolbar.IsVisible, true); // TODO: maybe this needs to go through the ViewController (top one?)
			}
		}

		public static void MapBackButtonVisible(IToolbarHandler handler, IToolbar toolbar)
		{
			handler.PlatformView.UpdateBackButtonVisibility(toolbar.BackButtonVisible);
		}
	}
}
