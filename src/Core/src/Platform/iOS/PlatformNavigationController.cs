﻿using System;
using System.Collections.Generic;
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

	
}

internal class NavigationDelegate : UINavigationControllerDelegate
{
	WeakReference<PlatformNavigationController> NavigationController { get; }

	public NavigationDelegate(PlatformNavigationController navigationController)
	{
		NavigationController = new WeakReference<PlatformNavigationController>(navigationController);
	}
}