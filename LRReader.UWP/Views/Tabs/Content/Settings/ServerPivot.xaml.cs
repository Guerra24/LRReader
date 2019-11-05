using LRReader.ViewModels;
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

namespace LRReader.Views.Tabs.Content.Settings
{
	public sealed partial class ServerPivot : PivotItem
	{
		private SettingsPageViewModel Data;

		public ServerPivot()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
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
			ClearNewFlyout.Hide();
		}
	}
}
