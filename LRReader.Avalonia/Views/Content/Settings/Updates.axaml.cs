using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Content.Settings;

public partial class Updates : ModernBasePage
{
	public SettingsPageViewModel Data { get; }

	public Updates()
	{
		InitializeComponent();
		Data = (SettingsPageViewModel)DataContext!;
	}
}