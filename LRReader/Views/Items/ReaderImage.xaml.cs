using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Items
{
	public sealed partial class ReaderImage : UserControl
	{
		private string _oldUrl = "";

		public ReaderImage()
		{
			this.InitializeComponent();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			string n = args.NewValue as string;
			if (!_oldUrl.Equals(n))
			{
				if (Global.SettingsManager.ImageCaching)
				{
					var image = await Global.ImageManager.DownloadImageCache(n);
					Image.Source = image;
					ScrollViewer.ChangeView(0, 0, Global.SettingsManager.BaseZoom);
				}
				else
				{
					var image = new BitmapImage();
					image.UriSource = new Uri(Global.SettingsManager.Profile.ServerAddress + "/" + n);
					Image.Source = image;
				}
				_oldUrl = n;
			}
		}

		private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var point = e.GetPosition(Image);
			var ttv = Image.TransformToVisual(this);
			var center = ttv.TransformPoint(new Point(0, 0));
			if (Math.Abs(ScrollViewer.ZoomFactor - Global.SettingsManager.BaseZoom) > 0.20)
				ScrollViewer.ChangeView(0, 0, Global.SettingsManager.BaseZoom);
			else
				ScrollViewer.ChangeView(point.X - center.X, point.Y, Global.SettingsManager.ZoomedFactor);
		}

		private void Image_ImageOpened(object sender, RoutedEventArgs e)
		{
			ScrollViewer.ChangeView(0, 0, Global.SettingsManager.BaseZoom);
		}

	}
}
