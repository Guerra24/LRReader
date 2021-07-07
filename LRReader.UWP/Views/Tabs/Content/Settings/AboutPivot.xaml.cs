using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Controls;
using LRReader.UWP.Views.Dialogs;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content.Settings
{
	public sealed partial class AboutPivot : PivotItem
	{
		private SettingsPageViewModel Data;

		private ResourceLoader lang = ResourceLoader.GetForCurrentView("Settings");

		public AboutPivot()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
#if SIDELOAD
			UpdateInfo.Visibility = Visibility.Visible;
#endif
		}

		private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
		{
			Data.UpdateReleaseData();
		}

		private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
			{
				await Service.Platform.OpenInBrowser(link);
			}
		}

		private async void ButtonDownload_Click(object sender, RoutedEventArgs e)
		{
			await Service.Platform.OpenInBrowser(new Uri(Data.ReleaseInfo.link));
			await ApplicationView.GetForCurrentView().TryConsolidateAsync();
		}

		private async void WebButton_Click(object sender, RoutedEventArgs e)
		{
			await Service.Platform.OpenInBrowser(new Uri((sender as ModernInput).Tag as string));
		}

		private async void License_Click(object sender, RoutedEventArgs e)
		{
			var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///LICENSE.md"));
			var dialog = new MarkdownDialog(lang.GetString("About/License"), await FileIO.ReadTextAsync(file));
			await dialog.ShowAsync();
		}

		private async void Privacy_Click(object sender, RoutedEventArgs e)
		{
			var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Privacy.md"));
			var dialog = new MarkdownDialog(lang.GetString("About/Privacy"), await FileIO.ReadTextAsync(file));
			await dialog.ShowAsync();
		}
	}
}
