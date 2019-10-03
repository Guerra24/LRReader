using LRReader.Internal;
using LRReader.Models.Main;
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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
{
	public sealed partial class ArchiveTabContent : UserControl
	{
		private ArchivePageViewModel Data;

		public ArchiveTabContent()
		{
			this.InitializeComponent();
			Data = new ArchivePageViewModel();
		}

		public async void LoadArchive(Archive archive)
		{
			Data.Archive = archive;
			Data.LoadTags();
			await Data.LoadImages();
		}

		private void ImagesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			//var animation = ImagesGrid.PrepareConnectedAnimation("imageReaderForward" + e.ClickedItem as string, e.ClickedItem, "Image");
			//animation.Configuration = new DirectConnectedAnimationConfiguration();
			Data.ShowReader = true;
			int i = Data.ArchiveImages.IndexOf(e.ClickedItem as string);
			int count = Data.ArchiveImages.Count;
			if (Global.SettingsManager.TwoPages)
			{
				if (i != 0)
				{
					--i; i /= 2; ++i;
				}
				if (Global.SettingsManager.ReadRTL)
				{
					--count; count /= 2; ++count;
					FlipView.SelectedIndex = count - i - (count % 2);
				}
				else
					FlipView.SelectedIndex = i;
			}
			else
			{
				if (Global.SettingsManager.ReadRTL)
					FlipView.SelectedIndex = count - i - 1;
				else
					FlipView.SelectedIndex = i;
			}

			if (Data.Archive.IsNewArchive())
			{
				Data.ClearNew();
				Data.Archive.isnew = "false";
			}
		}

		private async void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			Data.LoadTags();
			await Data.LoadImages();
		}

		private void FlipView_Loaded(object sender, RoutedEventArgs e)
		{
			FlipView.Focus(FocusState.Programmatic);
			// Let's remove the buttons
			var grid = (Grid)VisualTreeHelper.GetChild(FlipView, 0);
			for (int i = grid.Children.Count - 1; i >= 0; i--)
				if (grid.Children[i] is Button)
					grid.Children.RemoveAt(i);
		}

		private void FlipView_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var point = e.GetPosition(FlipView);
			double distance = FlipView.ActualWidth / 6.0;
			if (point.X < distance)
			{
				if (FlipView.SelectedIndex > 0)
					--FlipView.SelectedIndex;
			}
			else if (point.X > FlipView.ActualWidth - distance)
			{
				if (FlipView.SelectedIndex < FlipView.Items.Count - 1)
					++FlipView.SelectedIndex;
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			if (Data.ShowReader)
				Data.ShowReader = false;
		}

		private async void EditButton_Click(object sender, RoutedEventArgs e)
		{
			await Util.OpenInBrowser(new Uri(Global.SettingsManager.Profile.ServerAddress + "/edit?id=" + Data.Archive.arcid));
		}

		private async void DonwloadButton_Click(object sender, RoutedEventArgs e)
		{
			Data.Downloading = true;
			var download = await Data.DownloadArchive();

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

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				Data.LoadTags();
				await Data.LoadImages(false);
			}
		}


		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			Data.LoadTags();
			await Data.LoadImages();
		}
	}
}
