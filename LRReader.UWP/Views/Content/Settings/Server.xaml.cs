using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Server : ModernBasePage
	{
		private SettingsPageViewModel Data;

		public Server()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
		}

	}
}
