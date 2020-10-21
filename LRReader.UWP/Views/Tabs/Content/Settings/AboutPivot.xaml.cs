using LRReader.Internal;
using LRReader.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
	}
}
