﻿using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Server : ModernBasePage
	{
		private SettingsPageViewModel Data;

		public Server()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();

			switch (ActualTheme)
			{
				case ElementTheme.Light:
					Logo.Source = GetIcon("ms-appx:///Assets/Other/LANraragi-light.png");
					break;
				case ElementTheme.Dark:
					Logo.Source = GetIcon("ms-appx:///Assets/Other/LANraragi-dark.png");
					break;
			}
		}

		private void ModernBasePage_ActualThemeChanged(FrameworkElement sender, object args)
		{
			switch (ActualTheme)
			{
				case ElementTheme.Light:
					Logo.Source = GetIcon("ms-appx:///Assets/Other/LANraragi-light.png");
					break;
				case ElementTheme.Dark:
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

		private void ClearAllNewButton_Click(object sender, RoutedEventArgs e)
		{
			ClearNewFlyout.Hide();
		}

		private void RegenThumb_Click(object sender, RoutedEventArgs e)
		{
			RegenThumbsFlyout.Hide();
		}

		private void Repair_Click(object sender, RoutedEventArgs e)
		{
			RepairFlyout.Hide();
		}
	}
}
