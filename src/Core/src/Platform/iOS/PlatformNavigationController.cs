using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform;

internal class PlatformNavigationController : UINavigationController
{
	NavigationViewHandler Handler { get; }

	public PlatformNavigationController(NavigationViewHandler handler)
	{
		Handler = handler;
		Delegate = new NavigationDelegate(this);
	}

	[Export("navigationBar:shouldPopItem:")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Specific to instance")]
	protected bool ShouldPopItem(UINavigationBar _, UINavigationItem __)
	{
		//_uiRequestedPop = true;
		// SendBackButtonPressed();?
		return true;
	}
}

//MauiContext?.GetPlatformWindow().GetWindow();

internal class NavigationDelegate : UINavigationControllerDelegate
{
	WeakReference<PlatformNavigationController> NavigationController { get; }

	public NavigationDelegate(PlatformNavigationController navigationController)
	{
		NavigationController = new WeakReference<PlatformNavigationController>(navigationController);
	}
}