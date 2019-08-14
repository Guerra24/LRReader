using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
	public sealed partial class ArchiveImage : UserControl
	{
		private string _oldUrl = "";

		public ArchiveImage()
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
				Image.Visibility = Visibility.Collapsed;
				Ring.Visibility = Visibility.Visible;
				if (Global.SettingsManager.ImageCaching)
				{
					var image = await Global.ImageManager.DownloadImageCache(n);
					image.DecodePixelWidth = 200;
					Image.Source = image;
					Image.Visibility = Visibility.Visible;
					Ring.Visibility = Visibility.Collapsed;
				}
				else
				{
					var image = new BitmapImage();
					image.DecodePixelWidth = 200;
					image.UriSource = new Uri(Global.SettingsManager.Profile.ServerAddress + "/" + n);
					Image.Source = image;
				}
				_oldUrl = n;
			}
		}

		private void Image_ImageOpened(object sender, RoutedEventArgs e)
		{
			Image.Visibility = Visibility.Visible;
			Ring.Visibility = Visibility.Collapsed;
		}
	}
}
