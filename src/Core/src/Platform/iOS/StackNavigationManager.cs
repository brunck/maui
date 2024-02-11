using System.Collections.Generic;
using System;
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
		var previousNavigationStack = NavigationStack;
		var previousNavigationStackCount = previousNavigationStack.Count;
		bool initialNavigation = NavigationStack.Count == 0;

		if (request.NavigationStack.Count == 0)
		{
			throw new InvalidOperationException("NavigationStack cannot be empty");
		}

		if (NavigationController == null)
		{
			throw new InvalidOperationException("NavigationController cannot be null");
		}

		// This is the first navigation request, so just push the page(s) onto the stack
		if (initialNavigation)
		{
			NavigationStack = newPageStack;
			PushPages(request);
			NavigationView?.NavigationFinished(NavigationStack);
			return;
		}

		// TODO: modifying stack but not the currently visible page
	}

	private void PushPages(NavigationRequest request)
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
				throw new InvalidOperationException($"Page Handler must be a {nameof(PageHandler)}.");
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
					throw new InvalidOperationException($"Page Handler must be a {nameof(PageHandler)}.");
				}
			}
			NavigationController!.ViewControllers = [.. newStack];
		}
		// else 
	}
}
