using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Content.Settings;

public partial class Profiles : ModernBasePage
{
	public SettingsPageViewModel Data { get; }

	public Profiles()
	{
		InitializeComponent();
		DataContext = Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
		var lang = ResourceLoader.GetForCurrentView("Settings");
		SessionMode.Items.Add(lang.GetString("Profiles/SessionMode/Never"));
		SessionMode.Items.Add(lang.GetString("Profiles/SessionMode/Ask"));
		SessionMode.Items.Add(lang.GetString("Profiles/SessionMode/Always"));
	}

	private async void OpenFolder_Click(object sender, RoutedEventArgs e)
	{
		await TopLevel.GetTopLevel(this)!.Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Path.GetDirectoryName(Data.SettingsManager.ProfilesPathLocation)!));
	}
}