using LRReader.Internal;
using LRReader.Models.Main;
using LRReader.ViewModels.Items;
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

namespace LRReader.Views.Items
{
	public sealed partial class ArchiveItem : UserControl
	{

		private ArchiveItemViewModel ViewModel;

		private string _oldID = "";

		public ArchiveItem()
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
				Thumbnail.Visibility = Visibility.Collapsed;
				Ring.Visibility = Visibility.Visible;
				/*StorageFile file = await Global.ImageManager.DownloadThumbnailAsync(Archive.arcid);

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
					Title.Opacity = 1;
				}*/
				using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
				{
					byte[] bytes = await Global.ImageManager.DownloadThumbnailRuntime(ViewModel.Archive.arcid);
					await stream.WriteAsync(bytes.AsBuffer());
					stream.Seek(0);
					var image = new BitmapImage();
					await image.SetSourceAsync(stream);
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
				}
				Thumbnail.Visibility = Visibility.Visible;
				Ring.Visibility = Visibility.Collapsed;
				Title.Opacity = 1;
				_oldID = ViewModel.Archive.arcid;
			}
		}

		private async void EditMenuItem_Click(object sender, RoutedEventArgs e)
		{
			await Util.OpenInBrowser(new Uri(Global.SettingsManager.Profile.ServerAddress + "/edit?id=" + ViewModel.Archive.arcid));
		}

		private async void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var download = await ViewModel.DownloadArchive();

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
			savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
			savePicker.SuggestedFileName = download.Name;

			StorageFile file = await savePicker.PickSaveFileAsync();
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
	}
}
