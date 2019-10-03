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
using Windows.UI.Xaml.Media.Animation;
using LRReader.Models.Main;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Items
{
	public sealed partial class ReaderImage : UserControl
	{

		public ReaderImage()
		{
			this.InitializeComponent();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ArchiveImageSet n = args.NewValue as ArchiveImageSet;
			BitmapImage lImage = null;
			BitmapImage rImage = null;
			if (Global.SettingsManager.ImageCaching)
			{
				if (!string.IsNullOrEmpty(n.LeftImage))
					lImage = await Global.ImageManager.DownloadImageCache(n.LeftImage);
				if (!string.IsNullOrEmpty(n.RightImage))
					rImage = await Global.ImageManager.DownloadImageCache(n.RightImage);
				//var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("imageReaderForward" + n);
				//anim?.TryStart(Image);
			}
			else
			{
				if (!string.IsNullOrEmpty(n.LeftImage))
				{
					lImage = new BitmapImage();
					lImage.UriSource = new Uri(Global.SettingsManager.Profile.ServerAddress + "/" + n.LeftImage);
				}
				if (!string.IsNullOrEmpty(n.RightImage))
				{
					rImage = new BitmapImage();
					rImage.UriSource = new Uri(Global.SettingsManager.Profile.ServerAddress + "/" + n.RightImage);
				}
			}
			LeftImage.Source = lImage;
			RightImage.Source = rImage;
			if (Global.SettingsManager.ImageCaching)
				ScrollViewer.ChangeView(0, 0, Global.SettingsManager.BaseZoom, true);
		}

		private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var point = e.GetPosition(ScrollViewer);
			var ttv = ImagesRoot.TransformToVisual(this);
			var center = ttv.TransformPoint(new Point(0, 0));
			Debug.WriteLine(center.X);
			Debug.WriteLine(point.X);
			var zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ImagesRoot.ActualWidth, ScrollViewer.ViewportHeight / ImagesRoot.ActualHeight);
			if (Math.Abs(ScrollViewer.ZoomFactor - zoomFactor * Global.SettingsManager.BaseZoom) > 0.20)
				ScrollViewer.ChangeView(0, 0, zoomFactor * Global.SettingsManager.BaseZoom);
			else
				ScrollViewer.ChangeView(point.X - center.X * 2.0, point.Y, zoomFactor * Global.SettingsManager.ZoomedFactor);
		}

		private void Image_ImageOpened(object sender, RoutedEventArgs e)
		{
			//var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("imageReaderForward" + _oldUrl);
			//anim?.TryStart(Image);
			if (ImagesRoot.ActualWidth == 0 && ImagesRoot.ActualHeight == 0)
				return;
			var zoomFactor = (float)Math.Min(ScrollViewer.ViewportWidth / ImagesRoot.ActualWidth, ScrollViewer.ViewportHeight / ImagesRoot.ActualHeight);
			ScrollViewer.ChangeView(0, 0, zoomFactor * Global.SettingsManager.BaseZoom, true);
		}

	}
}
