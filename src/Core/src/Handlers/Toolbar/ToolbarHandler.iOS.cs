using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, UINavigationBar>
	{
		public IPlatformViewHandler? ParentPlatformViewHandler
		{
			get
			{
				if (VirtualView.Parent is IView view && view.Handler is IPlatformViewHandler parentHandler)
				{
					return parentHandler;
				}

				return null;
			}
		}

		protected override UINavigationBar CreatePlatformElement()
		{
			// this will blow up of course if the UINavigationController hasn't been set yet
			// TODO: or ParentPlatformViewHandler?.ViewController is UINavigationController navigationController then return navigationController.NavigationBar
			return ParentPlatformViewHandler?.ViewController?.NavigationController?.NavigationBar ?? throw new NullReferenceException("Could not obtain NavigationBar.");
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}
