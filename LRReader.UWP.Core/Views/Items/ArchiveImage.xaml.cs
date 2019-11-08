using GalaSoft.MvvmLight;
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
		private Container Data = new Container();

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
				Data.MissingImage = false;
				var image = await Global.ImageManager.DownloadImage(n);
				if (image != null)
				{
					image.DecodePixelWidth = 200;
					Image.Source = image;
				} else
				{
					Image.Source = null;
					Data.MissingImage = true;
				}
				Image.Visibility = Visibility.Visible;
				Ring.Visibility = Visibility.Collapsed;
				_oldUrl = n;
			}
		}

		private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
			Data.MissingImage = true;
		}

		private class Container : ObservableObject
		{
			private bool _missingImage;
			public bool MissingImage
			{
				get => _missingImage;
				set
				{
					_missingImage = value;
					RaisePropertyChanged("MissingImage");
				}
			}
		}
	}

}
