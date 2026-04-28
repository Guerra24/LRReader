using Avalonia.Interactivity;
using Avalonia.Platform;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;
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
		using var stream = AssetLoader.Open(new Uri("avares://LRReader.Avalonia/LICENSE.md"));
		using var reader = new StreamReader(stream);
		var content = await reader.ReadToEndAsync();
		await Service.Platform.OpenDialog(Dialog.Markdown, lang.GetString("About/License/Content"), content);
	}

	private async void Privacy_Click(object sender, RoutedEventArgs e)
	{
		using var stream = AssetLoader.Open(new Uri("avares://LRReader.Avalonia/Privacy.md"));
		using var reader = new StreamReader(stream);
		var content = await reader.ReadToEndAsync();
		await Service.Platform.OpenDialog(Dialog.Markdown, lang.GetString("About/Privacy/Content"), content);
	}
}