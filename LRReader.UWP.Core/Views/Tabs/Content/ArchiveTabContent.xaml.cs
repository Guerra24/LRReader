using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.ViewModels;
using LRReader.Views.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
{
	public sealed partial class ArchiveTabContent : UserControl
	{
		public ArchivePageViewModel Data;

		private int i;
		private bool _wasNew;

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			Data = new ArchivePageViewModel();
			Global.EventManager.RebuildReaderImagesSetEvent += Data.CreateImageSets;
			FadeOutReader.Begin(); // Hack to fade in correctly
			FadeOutReader.Completed += FadeOutReader_Completed;
			FadeOutContent.Completed += FadeOutContent_Completed;
		}

		private void FadeOutContent_Completed(object sender, object e)
		{
			FadeInReader.Begin();
			int count = Data.Pages;
			if (Global.SettingsManager.TwoPages)
			{
				if (i != 0)
				{
					--i; i /= 2; ++i;
				}
				if (Global.SettingsManager.ReadRTL)
				{
					int preDiv = count;
					--count; count /= 2; ++count;
					FlipViewControl.SelectedIndex = count - i - (preDiv % 2);
				}
				else
					FlipViewControl.SelectedIndex = i;
			}
			else
			{
				if (Global.SettingsManager.ReadRTL)
					FlipViewControl.SelectedIndex = count - i - 1;
				else
					FlipViewControl.SelectedIndex = i;
			}

			if (Data.Archive.IsNewArchive())
			{
				Data.ClearNew();
				Data.Archive.isnew = "false";
				_wasNew = true;
			}
			FlipViewControl.Focus(FocusState.Programmatic);
		}

		private void FadeOutReader_Completed(object sender, object e)
		{
			Data.ShowReader = false;
			FadeInContent.Begin();
		}

		public void LoadArchive(Archive archive)
		{
			Data.Archive = archive;
			Data.Reload(true);
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			FadeOutContent.Begin();
			Data.ShowReader = true;
			i = Data.ArchiveImages.IndexOf(e.ClickedItem as string);
		}

		private void Continue_Click(object sender, RoutedEventArgs e)
		{
			FadeOutContent.Begin();
			Data.ShowReader = true;
			i = Data.BookmarkProgress;
		}

		private void FlipView_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var point = e.GetPosition(FlipViewControl);
			double distance = FlipViewControl.ActualWidth / 6.0;
			if (point.X < distance)
			{
				if (FlipViewControl.SelectedIndex > 0)
					--FlipViewControl.SelectedIndex;
			}
			else if (point.X > FlipViewControl.ActualWidth - distance)
			{
				if (FlipViewControl.SelectedIndex < FlipViewControl.Items.Count - 1)
					++FlipViewControl.SelectedIndex;
			}
		}

		private async void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			if (Data.ShowReader)
				FadeOutReader.Begin();
			int conv = FlipViewControl.SelectedIndex;
			int count = Data.Pages;
			if (Global.SettingsManager.TwoPages)
			{
				if (conv != 0)
				{
					conv *= 2;
				}
				if (Global.SettingsManager.ReadRTL)
				{
					conv = count - conv - (count % 2);
				}
			}
			else
			{
				if (Global.SettingsManager.ReadRTL)
					conv = count - conv - 1;
			}
			conv = Math.Clamp(conv, 0, count - 1);
			if (conv >= count - 1)
			{
				if (Data.Bookmarked && Global.SettingsManager.RemoveBookmark)
				{
					var dialog = new ContentDialog { Title = "Remove bookmark?", PrimaryButtonText = "Yes", CloseButtonText = "No" };
					var result = await dialog.ShowAsync();
					if (result == ContentDialogResult.Primary)
						Data.Bookmarked = false;
				}
			}
			else
			{
				if (_wasNew && Global.SettingsManager.BookmarkReminder)
				{
					var dialog = new ContentDialog { Title = "Bookmark archive?", PrimaryButtonText = "Yes", CloseButtonText = "No" };
					var result = await dialog.ShowAsync();
					if (result == ContentDialogResult.Primary)
						Data.Bookmarked = true;
					_wasNew = false;
				}
			}
			if (Data.Bookmarked)
				Data.BookmarkProgress = conv;
		}

		private async void EditButton_Click(object sender, RoutedEventArgs e)
		{
			await Util.OpenInBrowser(new Uri(Global.SettingsManager.Profile.ServerAddressBrowser + "/edit?id=" + Data.Archive.arcid));
		}

		private async void DonwloadButton_Click(object sender, RoutedEventArgs e)
		{
			Data.Downloading = true;
			var download = await Data.DownloadArchive();
			if (download == null)
			{
				Data.Downloading = false;
				return;
			}

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
			savePicker.FileTypeChoices.Add(download.Type + " File", new List<string>() { download.Type });
			savePicker.SuggestedFileName = download.Name;

			StorageFile file = await savePicker.PickSaveFileAsync();
			Data.Downloading = false;
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

		private void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				Data.Reload(false);
			}
		}

		private void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			Data.Reload(true);
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			Data.Reload(true);
		}

		public void RemoveEvent()
		{
			Global.EventManager.RebuildReaderImagesSetEvent -= Data.CreateImageSets;
		}

	}
}
