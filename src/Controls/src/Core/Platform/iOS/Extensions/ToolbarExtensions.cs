using System;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		internal static void UpdateTitleArea(this Toolbar toolbar)
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

		internal static void UpdateBarBackground(this UINavigationBar navigationBar, Toolbar toolbar)
		{
			var barBackgroundBrush = toolbar.BarBackground;
			Graphics.Color? barBackgroundColor = null;

			// if the brush has a solid color, treat it as a Color, so we can compute the alpha value
			if (barBackgroundBrush is SolidColorBrush scb)
			{
				barBackgroundColor = scb.Color;
				barBackgroundBrush = null;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				var navigationBarAppearance = navigationBar.StandardAppearance;
				if (barBackgroundColor is null)
				{
					navigationBarAppearance.ConfigureWithOpaqueBackground();
					navigationBarAppearance.BackgroundColor = ColorExtensions.BackgroundColor;
					navigationBar.SetupDefaultNavigationBarAppearance();
				}
				else
				{
					if (barBackgroundColor.Alpha < 1f)
					{
						navigationBarAppearance.ConfigureWithTransparentBackground();
					}
					else
					{
						navigationBarAppearance.ConfigureWithOpaqueBackground();
					}

					navigationBarAppearance.BackgroundColor = barBackgroundColor.ToPlatform();
				}

				if (barBackgroundBrush is not null)
				{
					var backgroundImage = navigationBar.GetBackgroundImage(barBackgroundBrush);

					navigationBarAppearance.BackgroundImage = backgroundImage;
				}

				navigationBar.CompactAppearance = navigationBarAppearance;
				navigationBar.StandardAppearance = navigationBarAppearance;
				navigationBar.ScrollEdgeAppearance = navigationBarAppearance;
			}
			else
			{
				if (barBackgroundColor?.Alpha == 0f)
				{
					navigationBar.SetTransparentNavigationBar();
				}
				else
				{
					// Set navigation bar background color
					navigationBar.BarTintColor = barBackgroundColor == null
						? UINavigationBar.Appearance.BarTintColor
						: barBackgroundColor.ToPlatform();

					var backgroundImage = navigationBar.GetBackgroundImage(barBackgroundBrush);
					navigationBar.SetBackgroundImage(backgroundImage, UIBarMetrics.Default);
				}
			}
		}

		internal static void SetupDefaultNavigationBarAppearance(this UINavigationBar navBar)
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13)))
			{
				return;
			}

			// We will use UINavigationBar.Appearance to infer settings that
			// were already set to the navigation bar in older versions of
			// iOS.
			UINavigationBarAppearance navAppearance = navBar.StandardAppearance;

			if (navAppearance.BackgroundColor == null)
			{
				UIColor? backgroundColor = navBar.BarTintColor;

				navBar.StandardAppearance.BackgroundColor = backgroundColor;

				if (navBar.ScrollEdgeAppearance != null)
				{
					navBar.ScrollEdgeAppearance.BackgroundColor = backgroundColor;
				}

				if (navBar.CompactAppearance != null)
				{
					navBar.CompactAppearance.BackgroundColor = backgroundColor;
				}
			}

			if (navAppearance.BackgroundImage == null)
			{
				UIImage backgroundImage = navBar.GetBackgroundImage(UIBarMetrics.Default);

				navBar.StandardAppearance.BackgroundImage = backgroundImage;

				if (navBar.ScrollEdgeAppearance != null)
				{
					navBar.ScrollEdgeAppearance.BackgroundImage = backgroundImage;
				}

				if (navBar.CompactAppearance != null)
				{
					navBar.CompactAppearance.BackgroundImage = backgroundImage;
				}
			}

			if (navAppearance.ShadowImage == null)
			{
				UIImage? shadowImage = navBar.ShadowImage;
				UIColor clearColor = UIColor.Clear;

				navBar.StandardAppearance.ShadowImage = shadowImage;

				if (navBar.ScrollEdgeAppearance != null)
				{
					navBar.ScrollEdgeAppearance.ShadowImage = shadowImage;
				}

				if (navBar.CompactAppearance != null)
				{
					navBar.CompactAppearance.ShadowImage = shadowImage;
				}

				if (shadowImage != null && shadowImage.Size == SizeF.Empty)
				{
					navBar.StandardAppearance.ShadowColor = clearColor;

					if (navBar.ScrollEdgeAppearance != null)
					{
						navBar.ScrollEdgeAppearance.ShadowColor = clearColor;
					}

					if (navBar.CompactAppearance != null)
					{
						navBar.CompactAppearance.ShadowColor = clearColor;
					}
				}
			}

			UIImage? backIndicatorImage = navBar.BackIndicatorImage;
			UIImage? backIndicatorTransitionMaskImage = navBar.BackIndicatorTransitionMaskImage;

			if (backIndicatorImage != null && backIndicatorImage.Size == SizeF.Empty)
			{
				backIndicatorImage = GetEmptyBackIndicatorImage();
			}

			if (backIndicatorTransitionMaskImage != null && backIndicatorTransitionMaskImage.Size == SizeF.Empty)
			{
				backIndicatorTransitionMaskImage = GetEmptyBackIndicatorImage();
			}

			navBar.CompactAppearance?.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
			navBar.StandardAppearance.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
			navBar.ScrollEdgeAppearance?.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
		}

		internal static UIImage GetEmptyBackIndicatorImage()
		{
			var rect = RectangleF.Empty;
			SizeF size = rect.Size;

			UIGraphics.BeginImageContext(size);
			CGContext? context = UIGraphics.GetCurrentContext();
			context?.SetFillColor(1, 1, 1, 0);
			context?.FillRect(rect);

			UIImage? empty = UIGraphics.GetImageFromCurrentImageContext();
			context?.Dispose();

			return empty;
		}

		internal static ParentViewController? GetParentViewController(this UINavigationBar navigationBar)
		{
			var viewControllers = navigationBar.GetNavigationController()?.ViewControllers;
			if (!viewControllers?.Any() ?? true)
			{
				return null;
			}

			var parentViewController = viewControllers.Last() as ParentViewController;
			return parentViewController;
		}


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
			if (titleIcon.IsEmpty)
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
