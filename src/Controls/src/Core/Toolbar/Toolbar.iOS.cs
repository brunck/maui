using System;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
    {
        UINavigationController? NavigationController
        {
            get
            {
                if (Parent is IView view && view.Handler is IPlatformViewHandler parentHandler)
                {
                    return parentHandler.ViewController?.NavigationController;
                }
                return null;
            }
        }

        public static void MapIsVisible(IToolbarHandler handler, Toolbar toolbar)
        {
			if (toolbar.NavigationController == null)
			{
				throw new NullReferenceException("NavigationController is null.");
			}
            toolbar.NavigationController.SetNavigationBarHidden(!toolbar.IsVisible, false);
        }

        //public Toolbar()
        //{
        //    //if (Handler is ToolbarHandler handler)
        //    //{
        //    //    handler.ParentPlatformViewHandler
        //    //}

            

        //}
    }
}
