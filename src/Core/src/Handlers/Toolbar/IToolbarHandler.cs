﻿#if IOS || MACCATALYST
using PlatformView = UIKit.UINavigationController;
#elif MONOANDROID
using PlatformView = Google.Android.Material.AppBar.MaterialToolbar;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.MauiToolbar;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiToolbar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IToolbarHandler : IElementHandler
	{
		new IToolbar VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}