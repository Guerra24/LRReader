using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class About : Page
	{
		private SettingsPageViewModel Data;

		private ResourceLoader lang = ResourceLoader.GetForCurrentView("Settings");

		public About()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
		}

		private async void License_Click(object sender, RoutedEventArgs e)
		{
			var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///LICENSE.md"));
			await Service.Platform.OpenDialog(Dialog.Markdown, lang.GetString("About/License"), await FileIO.ReadTextAsync(file));
		}

		private async void Privacy_Click(object sender, RoutedEventArgs e)
		{
			var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Privacy.md"));
			await Service.Platform.OpenDialog(Dialog.Markdown, lang.GetString("About/Privacy"), await FileIO.ReadTextAsync(file));
		}

	}
}
