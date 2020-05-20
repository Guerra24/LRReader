using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.UWP.ViewModels.Base;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
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
	public sealed partial class CategoryItem : UserControl
	{
		private CategoryBaseViewModel ViewModel;

		public CategoryItem()
		{
			this.InitializeComponent();
			ViewModel = new CategoryBaseViewModel();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Category = args.NewValue as Category;
			Overlay.Opacity = 0;
			Title.Opacity = 0;
			Thumbnail.Source = null;
			Ring.IsActive = true;
			ViewModel.MissingImage = false;
			using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
			{
				var first = ViewModel.Category.archives.FirstOrDefault();
				if (first != null)
				{

					byte[] bytes = await Global.ImageManager.DownloadThumbnailRuntime(ViewModel.Category.archives.FirstOrDefault());
					if (bytes != null)
					{
						await stream.WriteAsync(bytes.AsBuffer());
						stream.Seek(0);
						var image = new BitmapImage();
						image.DecodePixelWidth = 200;
						await image.SetSourceAsync(stream);
						if (image.PixelHeight != 0 && image.PixelWidth != 0)
							if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
								Thumbnail.Stretch = Stretch.Uniform;
						Thumbnail.Source = image;
					}
					else
					{
						ViewModel.MissingImage = true;
					}
				}
				else
				{
					ViewModel.MissingImage = true;
				}
			}
			Ring.IsActive = false;
			Overlay.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
			Title.Fade(value: 1.0f, duration: 250, easingMode: EasingMode.EaseIn).Start();
		}
	}
}
