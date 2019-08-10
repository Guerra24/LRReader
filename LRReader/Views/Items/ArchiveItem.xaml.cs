using LRReader.Internal;
using LRReader.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
	public sealed partial class ArchiveItem : UserControl
	{

		private string _oldID = "";

		public ArchiveItem()
		{
			this.InitializeComponent();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			Archive n = args.NewValue as Archive;
			if (!_oldID.Equals(n.arcid))
			{
				Thumbnail.Visibility = Visibility.Collapsed;
				Ring.Visibility = Visibility.Visible;
				StorageFile file = await Global.ImageManager.DownloadThumbnailAsync(n.arcid);

				using (var ras = await file.OpenAsync(FileAccessMode.Read))
				{
					var image = new BitmapImage();
					await image.SetSourceAsync(ras);
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
					Thumbnail.Visibility = Visibility.Visible;
					Ring.Visibility = Visibility.Collapsed;
				}
				_oldID = n.arcid;
			}
		}

	}
}
