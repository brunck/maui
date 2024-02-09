using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Platform;

class PlatformNavigationController : UINavigationController
{
	NavigationViewHandler Handler { get; }

	IReadOnlyList<IView> NavigationStack { get; set; } = new List<IView>();
	IStackNavigationView? NavigationView { get; set; }

	public PlatformNavigationController(NavigationViewHandler handler)
	{
		Handler = handler;
		Delegate = new NavigationDelegate(this);
	}

	public virtual void Connect(IStackNavigationView virtualView)
	{
		NavigationView = virtualView;
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

		// This is the first navigation request, so just push the page(s) onto the stack
		if (initialNavigation)
		{
			NavigationStack = newPageStack;
			PushPages(request);
			NavigationView?.NavigationFinished(NavigationStack);
			return;
		}


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
				PushViewController(handler.ViewController, request.Animated);
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
			ViewControllers = [.. newStack];
		}
		// else 
	}

	class NavigationDelegate : UINavigationControllerDelegate
	{
		WeakReference<PlatformNavigationController> NavigationController { get; }

		public NavigationDelegate(PlatformNavigationController navigationController)
		{
			NavigationController = new WeakReference<PlatformNavigationController>(navigationController);
		}
	}
}