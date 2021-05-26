using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Items;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Windows.Devices.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class BookmarkedArchive : UserControl
	{
		private ArchiveItemViewModel ViewModel;

		private string _oldID = "";

		public BookmarkedArchive()
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
				Title.SetVisualOpacity(0);
				Progress.SetVisualOpacity(0);
				Thumbnail.SetVisualOpacity(0);
				Thumbnail.Source = null;
				ViewModel.MissingImage = false;

				var image = new BitmapImage();
				image.DecodePixelType = DecodePixelType.Logical;
				image.DecodePixelWidth = 200;
				image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(ViewModel.Archive.arcid), image) as BitmapImage;

				if (image != null)
				{
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
				}
				else
					ViewModel.MissingImage = true;

				if (Service.Platform.AnimationsEnabled)
				{
					Title.FadeIn();
					Progress.FadeIn();
					Thumbnail.FadeIn();
				}
				else
				{
					Title.SetVisualOpacity(1);
					Progress.SetVisualOpacity(1);
					Thumbnail.SetVisualOpacity(1);
				}
				_oldID = ViewModel.Archive.arcid;
			}
		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
		{
			Service.Tabs.OpenTab(Tab.Archive, false, ViewModel.Archive);
		}

		private void EditMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Service.Tabs.OpenTab(Tab.ArchiveEdit, ViewModel.Archive);
		}

		private async void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Downloading = true;
			var download = await ViewModel.DownloadArchive();
			if (download == null)
			{
				ViewModel.Downloading = false;
				return;
			}

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
			savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
			savePicker.SuggestedFileName = download.Name;

			StorageFile file = await savePicker.PickSaveFileAsync();
			ViewModel.Downloading = false;
			if (file != null)
			{
				CachedFileManager.DeferUpdates(file);
				await FileIO.WriteBytesAsync(file, download.Data);
				FileUpdateStatus status =
					await CachedFileManager.CompleteUpdatesAsync(file);
				if (status == FileUpdateStatus.Complete)
				{
					//save
				}
				else
				{
					// not saved
				}
			}
			else
			{
				//cancel
			}
		}

		private void Add_Click(object sender, RoutedEventArgs e) => ViewModel.Bookmarked = true;

		private void Remove_Click(object sender, RoutedEventArgs e) => ViewModel.Bookmarked = false;

		private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(this);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsMiddleButtonPressed)
				{
					Service.Tabs.OpenTab(Tab.Archive, false, ViewModel.Archive);
				}
			}
		}

		private async void CategoriesButton_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new CategoryArchive(ViewModel.Archive.arcid, ViewModel.Archive.title);
			await dialog.ShowAsync();
		}
	}
}
