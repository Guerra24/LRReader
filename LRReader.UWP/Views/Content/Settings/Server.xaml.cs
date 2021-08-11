using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Server : ModernBasePage
	{
		private SettingsPageViewModel Data;

		public Server()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;

			switch (this.ActualTheme)
			{
				case ElementTheme.Light:
					Logo.Source = GetIcon("ms-appx:///Assets/Other/LANraragi-light.png");
					break;
				case ElementTheme.Dark:
					Logo.Source = GetIcon("ms-appx:///Assets/Other/LANraragi-dark.png");
					break;
			}

			var Listener = new ThemeListener();
			Listener.ThemeChanged += Listener_ThemeChanged;
		}

		private void Listener_ThemeChanged(ThemeListener sender)
		{
			switch (sender.CurrentTheme)
			{
				case ApplicationTheme.Light:
					Logo.Source = GetIcon("ms-appx:///Assets/Other/LANraragi-light.png");
					break;
				case ApplicationTheme.Dark:
					Logo.Source = GetIcon("ms-appx:///Assets/Other/LANraragi-dark.png");
					break;
			}
		}

		private BitmapImage GetIcon(string uri)
		{
			var image = new BitmapImage();
			image.DecodePixelType = DecodePixelType.Logical;
			image.DecodePixelHeight = 20;
			image.UriSource = new Uri(uri);
			return image;
		}

		private void UploadArchive_Click(object sender, RoutedEventArgs e) => Service.Tabs.OpenTab(Tab.Web, Service.Settings.Profile.ServerAddressBrowser + "/upload");

		private void BatchTagging_Click(object sender, RoutedEventArgs e) => Service.Tabs.OpenTab(Tab.Web, Service.Settings.Profile.ServerAddressBrowser + "/batch");

		private void EditSettings_Click(object sender, RoutedEventArgs e) => Service.Tabs.OpenTab(Tab.Web, Service.Settings.Profile.ServerAddressBrowser + "/config");

		private void EditPlugins_Click(object sender, RoutedEventArgs e) => Service.Tabs.OpenTab(Tab.Web, Service.Settings.Profile.ServerAddressBrowser + "/config/plugins");

		private void Logs_Click(object sender, RoutedEventArgs e) => Service.Tabs.OpenTab(Tab.Web, Service.Settings.Profile.ServerAddressBrowser + "/logs");

		private async void RestartWorkerButton_Click(object sender, RoutedEventArgs e) => await Data.RestartWorker();

		private async void StopWorkerButton_Click(object sender, RoutedEventArgs e) => await Data.StopWorker();

		private async void DownloadDBButton_Click(object sender, RoutedEventArgs e)
		{
			var download = await Data.DownloadDB();
			if (download == null)
				return;

			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
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

		private async void ClearAllNewButton_Click(object sender, RoutedEventArgs e)
		{
			ClearNewFlyout.Hide();
			await Data.ClearAllNew();
		}

		private async void ResetSearchButton_Click(object sender, RoutedEventArgs e)
		{
			var btn = sender as Button;
			btn.IsEnabled = false;
			await Data.ResetSearch();
			btn.IsEnabled = true;
		}

		private async void RegenThumb_Click(object sender, RoutedEventArgs e)
		{
			RegenThumbsFlyout.Hide();
			await Data.RegenThumbnails((bool)RegenThumbForced.IsChecked);
		}
	}
}
