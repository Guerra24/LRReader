using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Updates : Page
	{
		private SettingsPageViewModel Data;

		public Updates()
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
	}
}
