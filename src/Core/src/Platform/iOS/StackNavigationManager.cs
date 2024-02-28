using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Platform;

public class StackNavigationManager
{
	IMauiContext MauiContext { get; }

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

		var currentNavStack = NavigationStack;
		var incomingNavStack = request.NavigationStack;
		var isInitialNavigation = currentNavStack.Count == 0;

		if (isInitialNavigation)
		{
			SyncNativeStackWithNewStack(request);
			NavigationStack = new List<IView>(request.NavigationStack);
			NavigationView?.NavigationFinished(NavigationStack);
			return;
		}

		if (currentNavStack.Count < incomingNavStack.Count)
		{
			NavigationController!.PushViewController(incomingNavStack[incomingNavStack.Count - 1].ToUIViewController(MauiContext), request.Animated);
			NavigationStack = new List<IView>(request.NavigationStack);
			NavigationView?.NavigationFinished(NavigationStack);
			return;
		}

		//if (currentNavStack.Count > incomingNavStack.Count)
		//{
		//	//// Pop to the target page
		//	//var targetIndex = currentNavStack.IndexOf(incomingNavStack[incomingNavStack.Count - 1]);
		//	//if (targetIndex == -1)
		//	//{
		//	//	throw new InvalidOperationException("The target page is not in the current navigation stack.");
		//	//}

		//	//var viewController = NavigationStack[targetIndex].ToUIViewController(MauiContext);
		//	//var targetViewController = viewController.ParentViewController;
		//	//NavigationController!.PopToViewController(targetViewController, request.Animated);
		//	NavigationController.PopViewController(request.Animated);
		//	NavigationStack = new List<IView>(request.NavigationStack);
		//	NavigationView?.NavigationFinished(NavigationStack);
		//	return;
		//}

		// The incoming and current stacks are the same length, so just sync the stacks
		SyncNativeStackWithNewStack(request);
		NavigationStack = new List<IView>(request.NavigationStack);
		NavigationView?.NavigationFinished(NavigationStack);
		return;
	}

	void SyncNativeStackWithNewStack(NavigationRequest request)
	{
		var newStack = new List<UIViewController>();
		foreach (var page in request.NavigationStack)
		{
			UIViewController? viewController = null;
			IPlatformViewHandler? handler = null;

			if (page is IElement element)
			{
				if (element.Handler is IPlatformViewHandler nvh && nvh.ViewController != null)
				{
					handler = nvh;
					viewController = nvh.ViewController;
				}
				else
				{
					handler = page.ToHandler(MauiContext);
					viewController = handler.ViewController;
				}

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

			newStack.Add(viewController);
		}
		NavigationController!.SetViewControllers([.. newStack], request.Animated);
	}
}
