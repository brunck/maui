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

	internal StackNavigationManager? NavigationManager { get; private set; }

	public IStackNavigationView NavigationView => VirtualView;

	public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

	PlatformNavigationController? _platformNavigationController;
	UIViewController? IPlatformViewHandler.ViewController => _platformNavigationController;

	protected override UIView CreatePlatformView()
	{
		_platformNavigationController ??= new PlatformNavigationController(this);
		NavigationManager = CreateNavigationManager();

		if (_platformNavigationController.View is null)
		{
			throw new NullReferenceException("PlatformNavigationController.View is null.");
		}

		return _platformNavigationController.View;
	}

	protected override void ConnectHandler(UIView platformView)
	{
		if (_platformNavigationController is null)
		{
			throw new NullReferenceException("PlatformNavigationController is null.");
		}

		NavigationManager?.Connect(VirtualView, _platformNavigationController);
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(UIView platformView)
	{
		NavigationManager?.Disconnect(VirtualView, _platformNavigationController!);
		base.DisconnectHandler(platformView);
	}

	void RequestNavigation(NavigationRequest request)
	{
		NavigationManager?.RequestNavigation(request);
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

	protected virtual StackNavigationManager CreateNavigationManager() =>
		new StackNavigationManager(MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null"));
}