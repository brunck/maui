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
		IReadOnlyList<IView> newPageStack = new List<IView>(request.NavigationStack);
		var existingNavigationStack = NavigationStack;
		bool initialNavigation = existingNavigationStack.Count == 0;

		if (request.NavigationStack.Count == 0)
		{
			throw new InvalidOperationException("NavigationStack cannot be empty.");
		}

		if (NavigationController == null)
		{
			throw new InvalidOperationException("NavigationController cannot be null.");
		}

		// This is the first navigation request, so just push the page(s) onto the stack
		if (initialNavigation)
		{
			NavigationStack = newPageStack;
			PushInitialPages(request);
			NavigationView?.NavigationFinished(NavigationStack);
			return;
		}

		var topOfNewStack = newPageStack[newPageStack.Count - 1];
		var topOfExistingStack = existingNavigationStack[existingNavigationStack.Count - 1];

		// The user has modified the navigation stack, but not the currently-visible page
		// So just sync the native stack with the new stack
		if (!initialNavigation && topOfNewStack == topOfExistingStack)
		{
			SyncNativeStackWithNewStack(newPageStack);
			NavigationStack = newPageStack;
			NavigationView?.NavigationFinished(NavigationStack);
			return;
		}

		/*
		 while (nativeStackCount != pageStack.Count)
			{
				if (nativeStackCount > pageStack.Count)
				{
					NavigationFrame.BackStack.RemoveAt(0);
				}
				else
				{
					NavigationFrame.BackStack.Insert(
						0, new PageStackEntry(GetDestinationPageType(), null, null));
				}

				nativeStackCount = NavigationFrame.BackStackDepth + 1;
			}
		 */

		//void SyncNativeStackWithNewStack(IReadOnlyList<IView> newStack)
		//{
		//	var pageControllers = NavigationController!.ViewControllers;
			
		//	if (pageControllers == null || pageControllers.Length == 0)
		//	{
		//		throw new InvalidOperationException($"{nameof(pageControllers)} {(pageControllers == null ? "is null." : "count is 0.")}");
		//	}

		//	var nativeStackCount = pageControllers.Length;

		//	while (nativeStackCount != newStack.Count)
		//	{
		//		if (nativeStackCount > newStack.Count)
		//		{
		//			pageControllers.RemoveAt(0);
		//		}
		//		else
		//		{
		//			var pageToInsert = newStack[newStack.Count - 1] ?? throw new InvalidOperationException("Navigation request has null elements.");

		//			if (pageToInsert.Handler is PageHandler handler)
		//			{
		//				if (handler.ViewController == null)
		//				{
		//					throw new InvalidOperationException("Page handler's ViewController cannot be null.");
		//				}
		//				pageControllers.Insert(0, handler.ViewController);
		//			}
		//		}

		//		nativeStackCount = pageControllers.Length;
		//	}

		//	NavigationController.ViewControllers = pageControllers;
		//}
	}

	private void PushInitialPages(NavigationRequest request)
	{
		if (request.NavigationStack.Count == 1)
		{
			var page = request.NavigationStack[0];
			if (page.Handler is PageHandler handler)
			{
				if (handler.ViewController == null)
				{
					throw new InvalidOperationException("Page handler's ViewController cannot be null.");
				}
				NavigationController!.PushViewController(handler.ViewController, request.Animated);
			}
			else
			{
				if (page.Handler is FlyoutViewHandler)
				{
					System.Diagnostics.Trace.WriteLine($"Pushing a FlyoutPage onto a NavigationPage is not a supported UI pattern on iOS. " +
						"Please see https://developer.apple.com/documentation/uikit/uisplitviewcontroller for more details.");
				}

				throw new InvalidOperationException($"Page's Handler must be a {nameof(PageHandler)}.");
			}
		}
		else if (request.NavigationStack.Count > 1)
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
}
