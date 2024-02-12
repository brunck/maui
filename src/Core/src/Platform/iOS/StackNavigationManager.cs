using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Platform;

public class StackNavigationManager
{
	public IMauiContext MauiContext { get; }

	IReadOnlyList<IView> NavigationStack { get; set; } = new List<IView>();
	IStackNavigationView? NavigationView { get; set; }
	UINavigationController? NavigationController { get; set; }

	public StackNavigationManager(IMauiContext mauiContext)
	{
		MauiContext = mauiContext;
	}

	public virtual void Connect(IStackNavigationView virtualView, UINavigationController navigationController)
	{
		NavigationView = virtualView;
		NavigationController = navigationController;
	}

	public virtual void Disconnect(IStackNavigationView virtualView, UINavigationController navigationController)
	{
		// TODO: anything else to clean up here
		NavigationView = null;
		NavigationController = null;
	}

	public virtual void RequestNavigation(NavigationRequest request)
	{
		if (request.NavigationStack.Count == 0)
		{
			throw new InvalidOperationException("NavigationStack cannot be empty.");
		}

		if (NavigationController == null)
		{
			throw new InvalidOperationException("NavigationController cannot be null.");
		}

		SyncNativeStackWithNewStack(request);
		NavigationStack = new List<IView>(request.NavigationStack);
		NavigationView?.NavigationFinished(NavigationStack);
		return;
	}

	private void SyncNativeStackWithNewStack(NavigationRequest request)
	{
		var newStack = new List<UIViewController>();
		foreach (var page in request.NavigationStack)
		{
			if (page.Handler is PageHandler handler)
			{
				if (handler.ViewController == null)
				{
					throw new InvalidOperationException("Page handler's ViewController cannot be null.");
				}
				newStack.Add(handler.ViewController);
			}
			else
			{
				if (page.Handler is FlyoutViewHandler)
				{
					System.Diagnostics.Trace.WriteLine($"Pushing a FlyoutPage onto a NavigationPage is not a supported UI pattern on iOS. " +
						"Please see https://developer.apple.com/documentation/uikit/uisplitviewcontroller for more details.");
				}
				throw new InvalidOperationException($"Page Handler must be a {nameof(PageHandler)}.");
			}
		}
		NavigationController!.SetViewControllers([.. newStack], request.Animated);
	}
}
