using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Handlers;

public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, UIView>, IPlatformViewHandler
{
	/*
	   [PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName] = NavigationPage.MapPrefersLargeTitles,
	   [PlatformConfiguration.iOSSpecific.NavigationPage.IsNavigationBarTranslucentProperty.PropertyName] = NavigationPage.MapIsNavigationBarTranslucent,
	 */

	internal StackNavigationManager? StackNavigationManager { get; private set; }

	internal NavigationManager? NavigationManager => MauiContext?.GetNavigationManager();

	public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

	PlatformNavigationController? _platformNavigationController;
	UIViewController? IPlatformViewHandler.ViewController => _platformNavigationController;

	protected override UIView CreatePlatformView()
	{
		_platformNavigationController ??= new PlatformNavigationController(this);
		StackNavigationManager = CreateStackNavigationManager();

		if (_platformNavigationController.View is null)
		{
			throw new NullReferenceException("PlatformNavigationController.View is null.");
		}

		NavigationManager?.SetNavigationController(_platformNavigationController);

		return _platformNavigationController.View;
	}

	protected override void ConnectHandler(UIView platformView)
	{
		if (_platformNavigationController is null)
		{
			throw new NullReferenceException("PlatformNavigationController is null.");
		}

		StackNavigationManager?.Connect(VirtualView, _platformNavigationController);
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(UIView platformView)
	{
		StackNavigationManager?.Disconnect(VirtualView, _platformNavigationController!);
		base.DisconnectHandler(platformView);
	}

	void RequestNavigation(NavigationRequest request)
	{
		StackNavigationManager?.RequestNavigation(request);
	}

	public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
	{
		if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest nr)
		{
			platformHandler.NavigationStack = nr.NavigationStack;
			platformHandler.RequestNavigation(nr);
		}
		else
		{
			throw new InvalidOperationException("Args must be NavigationRequest");
		}
	}

	public static void MapToolbar(IViewHandler handler, IView view)
	{
		if (handler.VirtualView is not IToolbarElement te || te.Toolbar == null)
		{
			return;
		}

		MapToolbar(handler, te);
	}

	internal static void MapToolbar(IElementHandler handler, IToolbarElement te)
	{
		if (te.Toolbar == null)
		{
			return;
		}

		_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by the base class.");

		// We don't need this return value but we need to realize the handler
		// otherwise the toolbar mapping doesn't work
		_ = te.Toolbar.ToHandler(handler.MauiContext);

		var navManager = handler.MauiContext.GetNavigationManager();
		navManager?.SetToolbarElement(te);
	}

	protected virtual StackNavigationManager CreateStackNavigationManager() =>
		new StackNavigationManager(MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null"));
}