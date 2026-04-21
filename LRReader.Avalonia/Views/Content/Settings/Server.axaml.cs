using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.ViewModels;

namespace LRReader.Avalonia.Views.Content.Settings;

public partial class Server : ModernBasePage
{
	public SettingsPageViewModel Data { get; }

	public Server()
	{
		InitializeComponent();
		Data = (SettingsPageViewModel)DataContext!;
	}
}