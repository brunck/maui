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
	public IStackNavigationView NavigationView => VirtualView;

	public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

	PlatformNavigationController? _platformNavigationController;
	UIViewController? IPlatformViewHandler.ViewController => _platformNavigationController; // TODO: figure out if this is correct - see NavigationRenderer.CreateViewControllerForPage()

	protected override UIView CreatePlatformView()
	{
		_platformNavigationController ??= new PlatformNavigationController(this);

		if (_platformNavigationController.View is null)
		{
			throw new NullReferenceException("PlatformNavigationController.View is null.");
		}

		return _platformNavigationController.View;
	}

	protected override void ConnectHandler(UIView platformView)
	{
		base.ConnectHandler(platformView);

		_platformNavigationController?.Connect(VirtualView);
	}

	void RequestNavigation(NavigationRequest request)
	{
		_platformNavigationController?.RequestNavigation(request);
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
}