using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static UIColor? GetBackgroundColor(this UIView view)
			=> view?.BackgroundColor;

		public static void UpdateIsEnabled(this UIView nativeView, IView view)
		{
			if (!(nativeView is UIControl uiControl))
				return;

			uiControl.Enabled = view.IsEnabled;
		}

		public static void UpdateBackgroundColor(this UIView nativeView, IView view)
		{
			if (nativeView == null)
				return;

			var color = view.BackgroundColor;

			if (color != null)
				nativeView.BackgroundColor = color.ToNative();
		}

		public static void UpdateAutomationId(this UIView nativeView, IView view) =>
			nativeView.AccessibilityIdentifier = view.AutomationId;

		public static void UpdateSemantics(this UIView nativeView, IView view)
		{
			var semantics = view.Semantics;
			if (semantics == null)
				return;

			nativeView.AccessibilityLabel = semantics.Description;
			nativeView.AccessibilityHint = semantics.Hint;

			if (semantics.IsHeading)
				nativeView.AccessibilityTraits |= UIAccessibilityTrait.Header;
			else
				nativeView.AccessibilityTraits &= ~UIAccessibilityTrait.Header;
		}

		public static T? FindDescendantView<T>(this UIView view) where T : UIView
		{
			var queue = new Queue<UIView>();
			queue.Enqueue(view);

			while (queue.Count > 0)
			{
				var descendantView = queue.Dequeue();

				if (descendantView is T result)
					return result;

				for (var i = 0; i < descendantView.Subviews?.Length; i++)
					queue.Enqueue(descendantView.Subviews[i]);
			}

			return null;
		}

		public static void InvalidateMeasure(this UIView nativeView, IView view)
		{
			nativeView.SetNeedsLayout();
		}

		public static void UpdateWidth(this UIView nativeView, IView view)
		{
			if (view.Width == -1)
			{
				// Ignore the initial set of the height; the initial layout will take care of it
				return;
			}

			UpdateFrame(nativeView, view);
		}

		public static void UpdateHeight(this UIView nativeView, IView view)
		{
			if (view.Height == -1)
			{
				// Ignore the initial set of the height; the initial layout will take care of it
				return;
			}

			UpdateFrame(nativeView, view);
		}

		public static void UpdateFrame(UIView nativeView, IView view)
		{
			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default (-1) width/height will be taken care of by GetDesiredSize
			var currentFrame = nativeView.Frame;
			nativeView.Frame = new CoreGraphics.CGRect(currentFrame.X, currentFrame.Y, view.Width, view.Height);
		}
	}
}