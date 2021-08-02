using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace LRReader.UWP.Extensions
{
	public static class Extension
	{

		public static void ClampPopup(this Popup popup, double horizontalOffset)
		{
			var point = popup.TransformToVisual(Window.Current.Content).TransformPoint(new Point(horizontalOffset, 0));
			if (point.X <= 0)
				popup.HorizontalOffset = Math.Max(horizontalOffset, horizontalOffset - point.X);
			else if (point.X + 310 > Window.Current.Bounds.Width)
				popup.HorizontalOffset = Math.Min(horizontalOffset, horizontalOffset - (point.X + 310 - Window.Current.Bounds.Width));
			else
				popup.HorizontalOffset = horizontalOffset;
		}
	}
}
