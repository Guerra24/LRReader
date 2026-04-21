using Avalonia.Interactivity;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Content.Settings;

public partial class About : ModernBasePage
{
	public SettingsPageViewModel Data { get; }

	private ResourceLoader lang = ResourceLoader.GetForCurrentView("Settings");

	public About()
	{
		InitializeComponent();
		Data = (SettingsPageViewModel)DataContext!;
	}

	private async void License_Click(object sender, RoutedEventArgs e)
	{
		//var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///LICENSE.md"));
		//await Service.Platform.OpenDialog(Dialog.Markdown, lang.GetString("About/License"), await FileIO.ReadTextAsync(file));
	}

	private async void Privacy_Click(object sender, RoutedEventArgs e)
	{
		//var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Privacy.md"));
		//await Service.Platform.OpenDialog(Dialog.Markdown, lang.GetString("About/Privacy"), await FileIO.ReadTextAsync(file));
	}
}