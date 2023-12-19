using System;
using CommunityToolkit.WinUI.Helpers;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Dialogs;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class About : Page
	{
		private SettingsPageViewModel Data;

		private ResourceLoader lang = ResourceLoader.GetForCurrentView("Settings");

		public About()
		{
			this.InitializeComponent();
			Data = (SettingsPageViewModel)DataContext;

			switch (this.ActualTheme)
			{
				case ElementTheme.Light:
					GithubLogo.Source = GetIcon("ms-appx:///Assets/Other/Github-light.png");
					break;
				case ElementTheme.Dark:
					GithubLogo.Source = GetIcon("ms-appx:///Assets/Other/Github-dark.png");
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
					GithubLogo.Source = GetIcon("ms-appx:///Assets/Other/Github-light.png");
					break;
				case ApplicationTheme.Dark:
					GithubLogo.Source = GetIcon("ms-appx:///Assets/Other/Github-dark.png");
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
