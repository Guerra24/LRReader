using LRReader.Internal;
using LRReader.UWP.ViewModels;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content.Settings
{
	public sealed partial class ServerPivot : PivotItem
	{
		private SettingsPageViewModel Data;

		public ServerPivot()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private void UploadArchive_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new WebTab(Global.SettingsManager.Profile.ServerAddressBrowser + "/upload"));

		private void BatchTagging_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new WebTab(Global.SettingsManager.Profile.ServerAddressBrowser + "/batch"));

		private void EditSettings_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new WebTab(Global.SettingsManager.Profile.ServerAddressBrowser + "/config"));

		private void EditPlugins_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new WebTab(Global.SettingsManager.Profile.ServerAddressBrowser + "/config/plugins"));

		private void Logs_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new WebTab(Global.SettingsManager.Profile.ServerAddressBrowser + "/logs"));

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
			await Data.ClearAllNew();
			ClearNewFlyout.Hide();
		}

		private async void ResetSearchButton_Click(object sender, RoutedEventArgs e)
		{
			var btn = sender as Button;
			btn.IsEnabled = false;
			await Data.ResetSearch();
			btn.IsEnabled = true;
		}
	}
}
