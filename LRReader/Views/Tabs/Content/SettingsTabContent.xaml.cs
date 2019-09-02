using LRReader.Internal;
using LRReader.ViewModels;
using LRReader.Views.Dialogs;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
{
	public sealed partial class SettingsTabContent : UserControl
	{
		private SettingsPageViewModel Data;

		public SettingsTabContent()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			await Data.UpdateCacheSize();
		}

		private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			ServerProfileDialog dialog = new ServerProfileDialog(false);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.AddProfile(dialog.ProfileName.Text, dialog.ProfileServerAddress.Text, dialog.ProfileServerApiKey.Password);
			}
		}

		private async void ButtonEdit_Click(object sender, RoutedEventArgs e)
		{
			ServerProfileDialog dialog = new ServerProfileDialog(true);
			ServerProfile profile = Data.SettingsManager.Profile;
			dialog.ProfileName.Text = profile.Name;
			dialog.ProfileServerAddress.Text = profile.ServerAddress;
			dialog.ProfileServerApiKey.Password = profile.ServerApiKey;

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.ModifyProfile(profile.UID, dialog.ProfileName.Text, dialog.ProfileServerAddress.Text, dialog.ProfileServerApiKey.Password);
				Global.LRRApi.RefreshSettings();
			}
		}

		private async void ButtonRemove_Click(object sender, RoutedEventArgs e)
		{
			ContentDialog dialog = new ContentDialog { Title = "Remove Profile?", PrimaryButtonText = "Yes", CloseButtonText = "No" };
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				var sm = Data.SettingsManager;
				sm.Profiles.Remove(sm.Profile);
				sm.Profile = null;
				sm.Profile = sm.Profiles.First();
			}
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Data.SettingsManager.Profile != null)
				Global.LRRApi.RefreshSettings();
		}

		private async void ButtonClearCache_Click(object sender, RoutedEventArgs e)
		{
			await Data.ClearCache();
			await Data.UpdateCacheSize();
		}

		private void RestartWorkerButton_Click(object sender, RoutedEventArgs e)
		{
			Data.RestartWorker();
		}

		private void StopWorkerButton_Click(object sender, RoutedEventArgs e)
		{
			Data.StopWorker();
		}

		private async void DownloadDBButton_Click(object sender, RoutedEventArgs e)
		{
			var download = await Data.DownloadDB();
			if (download == null)
				return;

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

		private void ClearAllNewButton_Click(object sender, RoutedEventArgs e)
		{
			Data.ClearAllNew();
		}
	}
}
