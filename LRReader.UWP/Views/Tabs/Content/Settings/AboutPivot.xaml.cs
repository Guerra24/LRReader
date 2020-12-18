using LRReader.Internal;
using LRReader.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content.Settings
{
	public sealed partial class AboutPivot : PivotItem
	{
		private SettingsPageViewModel Data;

		public AboutPivot()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
		{
			Data.UpdateReleaseData();
		}

		private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
			{
				await Util.OpenInBrowser(link);
			}
		}

		private async void ButtonDownload_Click(object sender, RoutedEventArgs e)
		{
			await Util.OpenInBrowser(new Uri(Data.ReleaseInfo.link));
			await ApplicationView.GetForCurrentView().TryConsolidateAsync();
		}
	}
}
