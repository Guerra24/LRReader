using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Items
{
	public sealed partial class ReaderImage : UserControl
	{
		public ReaderImage()
		{
			this.InitializeComponent();
		}

		private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var point = e.GetPosition(ScrollViewer);
			if (Math.Abs(ScrollViewer.ZoomFactor - Global.SettingsManager.BaseZoom) > 0.20)
				ScrollViewer.ChangeView(0, 0, Global.SettingsManager.BaseZoom);
			else
				ScrollViewer.ChangeView(point.X, point.Y, Global.SettingsManager.ZoomedFactor);
		}

		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Image.MaxWidth = ScrollViewer.ActualWidth;
			Image.MaxHeight = ScrollViewer.ActualHeight;
		}

		private void Image_ImageOpened(object sender, RoutedEventArgs e)
		{
			ScrollViewer.ChangeView(0, 0, Global.SettingsManager.BaseZoom);
		}
	}
}
