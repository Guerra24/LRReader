using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Providers;
using Microsoft.Toolkit.Uwp.UI.Animations;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Items
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
				Image.Opacity = 0;
				Ring.IsActive = true;
				Data.MissingImage = false;
				var image = await Util.ByteToBitmap(await SharedGlobal.ImagesManager.GetImageCached(n));
				if (image != null)
				{
					image.DecodePixelHeight = 275;
					Image.Source = image;
					if (image.UriSource == null)
					{
						Ring.IsActive = false;
						Image.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
					}
				}
				else
				{
					Image.Source = null;
					Data.MissingImage = true;
				}
				_oldUrl = n;
			}
		}

		private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
			Data.MissingImage = true;
			Ring.IsActive = false;
		}

		private void Image_ImageOpened(object sender, RoutedEventArgs e)
		{
			Ring.IsActive = false;
			Image.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
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
