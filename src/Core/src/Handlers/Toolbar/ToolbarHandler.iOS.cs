using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, UINavigationBar>
	{
		NavigationManager? NavigationManager => MauiContext?.GetNavigationManager();

		public UINavigationController NavigationController => NavigationManager?.NavigationController ?? throw new NullReferenceException("Could not obtain NavigationController.");

		protected override UINavigationBar CreatePlatformElement()
		{
			return NavigationManager?.NavigationController?.NavigationBar ?? throw new NullReferenceException("Could not obtain NavigationBar.");
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
			if (arg1 is ToolbarHandler toolbarHandler)
			{
				// The title of the previous page on the stack is messed up if we're doing a push, and this can mess up the back button title on the new top of the stack.
				// This happens due to MauiNavigationImpl.OnPushAsync() updating the handler properties
				// only in the first callback to NavigationPage.SendHandlerUpdateAsync() which is called before the platform navigation is done.
				// That first callback invokes property updates when the NavigationPage.InternalChildren is manipulated and when the NavigationPage.CurrentPage is set.
				// The subsequent callbacks passed to NavigationPage.SendHandlerUpdateAsync() don't do anything to update the handler properties.
				// TODO: Figure out why the stack is too big
				var navigationStack = toolbarHandler.NavigationController.ViewControllers;
				if (navigationStack != null && navigationStack.Length > 1)
				{
					var previousPage = navigationStack[navigationStack.Length - 2] as ParentViewController;
					if (previousPage != null && previousPage.Element.TryGetTarget(out var page) && page is ITitledElement titledElement)
					{
						previousPage.Title = titledElement.Title;
					}
				}

				toolbarHandler.NavigationController.TopViewController?.UpdateNavigationBarTitle(arg2.Title);
			}
		}

		public static void MapIsVisible(IToolbarHandler handler, IToolbar toolbar)
		{
			if (handler is ToolbarHandler toolbarHandler)
			{
				toolbarHandler.NavigationController.UpdateNavigationBarVisibility(toolbar.IsVisible, true); // TODO: maybe this needs to go through the ViewController (top one?)
			}
		}
	}
}
