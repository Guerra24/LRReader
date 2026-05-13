using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
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
			var types = Enum.GetNames(typeof(RegistryType));
			foreach (var type in types)
			{
				RegistryType.Items.Add(type);
			}
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
	}
}
