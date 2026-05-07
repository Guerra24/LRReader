using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Main
{
	public partial class FirstRunPage : UserControl
	{
		public SettingsPageViewModel Data { get; }

		public FirstRunPage()
		{
			InitializeComponent();
			DataContext = Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
		}

	}
}
