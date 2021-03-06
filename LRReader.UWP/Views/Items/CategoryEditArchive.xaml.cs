﻿using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class CategoryEditArchive : UserControl
	{

		private ArchiveItemViewModel ViewModel;

		private string _oldID = "";

		private bool _open;

		public CategoryEditArchive()
		{
			this.InitializeComponent();
			// TODO: Proper fix
			ViewModel = Service.Services.GetRequiredService<ArchiveItemViewModel>();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Archive = args.NewValue as Archive;

			if (!_oldID.Equals(ViewModel.Archive.arcid))
			{
				Overlay.SetVisualOpacity(0);
				Title.SetVisualOpacity(0);
				TagsGrid.SetVisualOpacity(0);
				Thumbnail.Source = null;
				ViewModel.MissingImage = false;

				var image = new BitmapImage();
				image.DecodePixelType = DecodePixelType.Logical;
				image.DecodePixelHeight = 138;
				image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(ViewModel.Archive.arcid), image) as BitmapImage;
				if (image != null)
				{
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
					Thumbnail.FadeIn();
				}
				else
					ViewModel.MissingImage = true;

				if (Service.Platform.AnimationsEnabled)
				{
					Overlay.FadeIn();
					Title.FadeIn();
					TagsGrid.FadeIn();
				}
				else
				{
					Overlay.SetVisualOpacity(1);
					Title.SetVisualOpacity(1);
					TagsGrid.SetVisualOpacity(1);
				}
				_oldID = ViewModel.Archive.arcid;
			}
		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
		{
			Service.Tabs.OpenTab(Tab.Archive, false, ViewModel.Archive);
		}

		private async void TagsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			_open = true;
			await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = true;
				if (Service.Platform.AnimationsEnabled)
					ShowPopup.Begin();
			}
		}

		private void TagsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			if (_open)
			{
				_open = false;
				TagsPopup.IsOpen = false;
			}
			if (TagsPopup.IsOpen)
			{
				if (Service.Platform.AnimationsEnabled)
					HidePopup.Begin();
				else
					TagsPopup.IsOpen = false;
			}
		}

		private void HidePopup_Completed(object sender, object e)
		{
			TagsPopup.IsOpen = false;
		}

		private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Tabs.OpenTab(Tab.Archive, false, ViewModel.Archive);
					e.Handled = true;
				}
			}
		}

	}
}
