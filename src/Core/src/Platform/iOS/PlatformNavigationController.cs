using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform;

internal class PlatformNavigationController : UINavigationController
{
	protected NavigationViewHandler Handler { get; }

	public PlatformNavigationController(NavigationViewHandler handler)
	{
		Handler = handler;
		Delegate = new NavigationDelegate(this);
	}

	[Export("navigationBar:shouldPopItem:")]
	protected virtual bool ShouldPopItem(UINavigationBar _, UINavigationItem __)
	{
		BackButtonClicked();
		return false; // the pop happens in the NavigationPage via the call above
	}

	protected virtual void BackButtonClicked()
	{
		var window = (Handler.MauiContext?.GetPlatformWindow().GetWindow()) ?? throw new InvalidOperationException("Could not obtain Window.");
		window.BackButtonClicked();
	}
}

internal class NavigationDelegate : UINavigationControllerDelegate
{
	WeakReference<PlatformNavigationController> NavigationController { get; }

	public NavigationDelegate(PlatformNavigationController navigationController)
	{
		NavigationController = new WeakReference<PlatformNavigationController>(navigationController);
	}
}