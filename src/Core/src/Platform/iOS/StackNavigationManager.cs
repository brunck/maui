﻿using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Platform;

public class StackNavigationManager
{
	IMauiContext MauiContext { get; }

	IReadOnlyList<IView> NavigationStack { get; set; } = [];
	IStackNavigationView? NavigationView { get; set; }
	UINavigationController? NavigationController { get; set; }
	NavigationViewHandler? NavigationViewHandler { get; set; }

	public StackNavigationManager(IMauiContext mauiContext)
	{
		MauiContext = mauiContext;
	}

	public virtual void Connect(IStackNavigationView virtualView, UINavigationController navigationController, NavigationViewHandler navigationViewHandler)
	{
		NavigationView = virtualView;
		NavigationController = navigationController;
		NavigationViewHandler = navigationViewHandler;
	}

	public virtual void Disconnect(IStackNavigationView virtualView, UINavigationController navigationController)
	{
		// TODO: anything else to clean up here
		NavigationView = null;
		NavigationController = null;
		NavigationViewHandler = null;
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

		var currentNavStack = NavigationStack;
		var incomingNavStack = request.NavigationStack;
		var isInitialNavigation = currentNavStack.Count == 0 && incomingNavStack.Count == 1;

		// if (isInitialNavigation || currentNavStack.Count < incomingNavStack.Count && incomingNavStack.Count - currentNavStack.Count == 1)
		// {
		// 	NavigationStack = new List<IView>(request.NavigationStack);
		// 	NavigationController!.PushViewController(incomingNavStack[incomingNavStack.Count - 1].ToUIViewController(MauiContext), request.Animated);
		// 	//NavigationView?.NavigationFinished(NavigationStack);
		// 	return;
		// }

		// if (currentNavStack.Count > incomingNavStack.Count && currentNavStack.Count - incomingNavStack.Count == 1)
		// {
		// 	var currentTop = currentNavStack[currentNavStack.Count - 1];
		// 	var incomingTop = incomingNavStack[incomingNavStack.Count - 1];

		// 	if (currentTop != incomingTop && currentNavStack.Count - incomingNavStack.Count == 1)
		// 	{
		// 		var topViewController = NavigationController!.TopViewController; // currentTop.ToUIViewController(MauiContext);
		// 		NavigationStack = new List<IView>(request.NavigationStack);
		// 		topViewController.NavigationController?.PopViewController(request.Animated);
		// 		//NavigationController!.PopViewController(request.Animated);
		// 		return;
		// 	}
		// }

		// The incoming and current stacks are the same length, multiple pages are being added/removed, or non-visible pages are being manipulated, so just sync the stacks
		NavigationStack = new List<IView>(request.NavigationStack);
		SyncNativeStackWithNewStack(request);
		return;
	}

	void SyncNativeStackWithNewStack(NavigationRequest request)
	{
		var newStack = new List<UIViewController>();
		foreach (var page in request.NavigationStack)
		{
			UIViewController? viewController = null;

			if (page is IElement element)
			{
				var handler = page.Handler;
				viewController = page.ToUIViewController(MauiContext);

				if (handler is FlyoutViewHandler flyoutHandler)
				{
					System.Diagnostics.Trace.WriteLine($"Pushing a FlyoutPage onto a NavigationPage is not a supported UI pattern on iOS. " +
						"Please see https://developer.apple.com/documentation/uikit/uisplitviewcontroller for more details.");
				}
			}
			else
			{
				throw new InvalidOperationException("Page must be an IElement");
			}

			if (viewController == null)
			{
				throw new InvalidOperationException("ViewController cannot be null.");
			}

			var containerViewController = new ParentViewController(NavigationViewHandler
				?? throw new InvalidOperationException($"Could not convert handler to {nameof(NavigationViewHandler)}"),
				page);
			containerViewController.View!.AddSubview(viewController.View!);
			containerViewController.AddChildViewController(viewController);

			FixTitle(containerViewController, element);

			newStack.Add(containerViewController);
		}

		NavigationController!.SetViewControllers([.. newStack], request.Animated);
	}

	private static void FixTitle(UIViewController viewController, IElement element)
	{
		// The title of the previous page from the top on the stack is messed up if we're doing a push, and this can mess up the back button title on the new top of the stack.
		// We'll just make sure the title is correct on every page in the stack.
		// This happens due to MauiNavigationImpl.OnPushAsync() updating the handler properties
		// only in the first callback to NavigationPage.SendHandlerUpdateAsync() which is called before the platform navigation is done.
		// That first callback invokes property updates when the NavigationPage.InternalChildren is manipulated and when the NavigationPage.CurrentPage is set.
		// The subsequent callbacks passed to NavigationPage.SendHandlerUpdateAsync() don't do anything to update the handler properties.
		// TODO: Figure out why the stack is too big
		if (element is ITitledElement titledElement)
		{
			viewController.Title = titledElement.Title;
		}
	}
}
