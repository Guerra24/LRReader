using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.UWP.ViewModels.Items;
using LRReader.UWP.Views.Tabs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.UWP.Views.Items
{
	public sealed partial class BookmarkedArchive : UserControl
	{
		private ArchiveItemViewModel ViewModel;

		private string _oldID = "";

		public BookmarkedArchive()
		{
			this.InitializeComponent();
			ViewModel = new ArchiveItemViewModel();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			ViewModel.Archive = args.NewValue as Archive;

			if (!_oldID.Equals(ViewModel.Archive.arcid))
			{
				Title.Opacity = 0;
				Thumbnail.Source = null;
				Thumbnail.Visibility = Visibility.Collapsed;
				ViewModel.MissingImage = false;
				using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
				{
					byte[] bytes = await Global.ImageManager.DownloadThumbnailRuntime(ViewModel.Archive.arcid);
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
						Thumbnail.Visibility = Visibility.Visible;
					}
					else
					{
						ViewModel.MissingImage = true;
					}
				}
				Title.Opacity = 1;
				_oldID = ViewModel.Archive.arcid;
			}
		}

		private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new ArchiveTab(ViewModel.Archive), false);
		}

		private void EditMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new WebTab(Global.SettingsManager.Profile.ServerAddressBrowser + "/edit?id=" + ViewModel.Archive.arcid));
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

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Bookmarked = true;
		}

		private void Remove_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Bookmarked = false;
		}
	}
}
