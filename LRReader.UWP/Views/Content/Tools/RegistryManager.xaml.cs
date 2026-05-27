using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.UWP.Views.Content.Tools
{
	public sealed partial class RegistryManager : ModernBasePage
	{
		public RegistryManagerViewModel Data;

		public RegistryManager()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<RegistryManagerViewModel>();
			var providers = Enum.GetNames(typeof(RegistryProvider));
			foreach (var type in providers)
			{
				RegistryProvider.Items.Add(type);
			}
		}

		private void Registries_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var registry = (Registry)e.AddedItems.First();
				Data.SelectRegistry(registry);
			}
			else
			{
				Data.SelectRegistry(null);
			}
		}

		private async void ModernBasePage_Loaded(object sender, RoutedEventArgs e)
		{
			await Data.LoadRegistries();
		}

		[DynamicWindowsRuntimeCast(typeof(Button))]
		private async void Install_Click(object sender, RoutedEventArgs e)
		{
			await Data.InstallPlugin((Registry)Registries.SelectedItem, (RegistryIndexPlugin)((Button)sender).Tag);
        }

		[DynamicWindowsRuntimeCast(typeof(Button))]
		private async void Uninstall_Click(object sender, RoutedEventArgs e)
		{
			await Data.UninstallPlugin((RegistryIndexPlugin)((Button)sender).Tag);
        }
    }
}
