using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		public static void UpdateTitleArea(this Toolbar toolbar)
		{
			ImageSource titleIcon = toolbar.TitleIcon;
			var titleView = toolbar.TitleView;

			ClearTitleViewContainer(toolbar);
			if (titleIcon == null || titleIcon.IsEmpty && titleView == null)
			{
				return;
			}

			if (toolbar.NavigationController == null)
			{
				throw new InvalidOperationException("NavigationController is null.");
			}
			NavigationTitleAreaContainer titleViewContainer = new NavigationTitleAreaContainer((View)titleView, toolbar.NavigationController.NavigationBar);

			UpdateTitleImage(titleViewContainer, titleIcon);
			toolbar.NavigationController.NavigationItem.TitleView = titleViewContainer;
		}

		/*
		 void UpdateBarTextColor()
		{
			var barTextColor = NavPage.BarTextColor;

			// Determine new title text attributes via global static data
			var globalTitleTextAttributes = UINavigationBar.Appearance.TitleTextAttributes;
			var titleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = barTextColor == null ? globalTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
				Font = globalTitleTextAttributes?.Font
			};

			// Determine new large title text attributes via global static data
			var largeTitleTextAttributes = titleTextAttributes;
			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				var globalLargeTitleTextAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;

				largeTitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = barTextColor == null ? globalLargeTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
					Font = globalLargeTitleTextAttributes?.Font
				};
			}

			if (OperatingSystem.IsIOSVersionAtLeast(13))
			{
				NavigationBar.CompactAppearance.TitleTextAttributes = titleTextAttributes;
				NavigationBar.CompactAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;

				NavigationBar.StandardAppearance.TitleTextAttributes = titleTextAttributes;
				NavigationBar.StandardAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;

				NavigationBar.ScrollEdgeAppearance.TitleTextAttributes = titleTextAttributes;
				NavigationBar.ScrollEdgeAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
			}
			else
			{
				NavigationBar.TitleTextAttributes = titleTextAttributes;

				if (OperatingSystem.IsIOSVersionAtLeast(11))
					NavigationBar.LargeTitleTextAttributes = largeTitleTextAttributes;
			}

			// set Tint color (i. e. Back Button arrow and Text)
			var iconColor = Current != null ? NavigationPage.GetIconColor(Current) : null;
			if (iconColor == null)
				iconColor = barTextColor;

			NavigationBar.TintColor = iconColor == null || NavPage.OnThisPlatform().GetStatusBarTextColorMode() == StatusBarTextColorMode.DoNotAdjust
				? UINavigationBar.Appearance.TintColor
				: iconColor.ToPlatform();
		}
		 */

		static void ClearTitleViewContainer(Toolbar toolbar)
		{
			var navigationItem = toolbar.NavigationController?.TopViewController.NavigationItem;
			if (navigationItem == null)
			{
				return;
			}

			if (navigationItem.TitleView != null && navigationItem.TitleView is NavigationTitleAreaContainer titleViewContainer)
			{
				titleViewContainer.Dispose();
				navigationItem.TitleView = null;
			}
		}

		static void UpdateTitleImage(NavigationTitleAreaContainer titleViewContainer, ImageSource titleIcon)
		{
			if (titleViewContainer == null)
			{
				return;
			}

			if (titleIcon == null || titleIcon.IsEmpty)
			{
				titleViewContainer.Icon = null;
			}
			else
			{
				var context = titleIcon.FindMauiContext() ?? throw new InvalidOperationException("MauiContext is null.");

				titleIcon.LoadImage(context, result =>
				{
					var image = result?.Value;
					try
					{
						titleViewContainer.Icon = new UIImageView(image);
					}
					catch
					{
						//UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
					}
				});
			}
		}
	}
}
