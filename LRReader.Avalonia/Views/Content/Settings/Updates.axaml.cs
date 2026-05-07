using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Content.Settings;

public partial class Updates : ModernBasePage
{
	public SettingsPageViewModel Data { get; }

	public Updates()
	{
		InitializeComponent();
		DataContext = Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
	}
}