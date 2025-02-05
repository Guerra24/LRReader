using System;
using System.Linq;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using LRReader.UWP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class TankoubonItem : UserControl
	{
		private TankoubonBaseViewModel ViewModel;

		private string _oldID = "";

		private ResourceLoader lang;

		public TankoubonItem()
		{
			this.InitializeComponent();
			// TODO: Proper fix
			ViewModel = Service.Services.GetRequiredService<TankoubonBaseViewModel>();
			lang = ResourceLoader.GetForCurrentView("Dialogs");
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Tankoubon = (Tankoubon)args.NewValue;

			if (!_oldID.Equals(ViewModel.Tankoubon.id))
			{
				Overlay.SetVisualOpacity(0);
				Title.SetVisualOpacity(0);
				Thumbnail.Source = null;
				ViewModel.SearchImage = false;
				ViewModel.MissingImage = false;

				var first = ViewModel.Tankoubon.archives.FirstOrDefault();
				if (first != null)
				{
					var image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(first), decodeHeight: 275) as BitmapImage;
					if (image == null)
						ViewModel.MissingImage = true;
					else
					{
						if (image.PixelHeight != 0 && image.PixelWidth != 0)
							if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
								Thumbnail.Stretch = Stretch.Uniform;
						Thumbnail.Source = image;
					}
				}
				else
				{
					ViewModel.SearchImage = true;
				}

				if (Service.Platform.AnimationsEnabled)
				{
					Overlay.FadeIn();
					Title.FadeIn();
				}
				else
				{
					Overlay.SetVisualOpacity(1);
					Title.SetVisualOpacity(1);
				}
				_oldID = ViewModel.Tankoubon.id;
			}
		}

		private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && pointerPoint.Properties.IsMiddleButtonPressed)
			{
				Service.Tabs.OpenTab(Tab.Tankoubon, false, ViewModel.Tankoubon);
				e.Handled = true;
			}
		}

	}
}
