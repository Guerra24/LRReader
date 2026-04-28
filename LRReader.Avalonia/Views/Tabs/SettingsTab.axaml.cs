using Avalonia.Interactivity;
using Avalonia.Threading;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Tabs
{
	public partial class SettingsTab : ModernTab
	{
		private SettingsPageViewModel Data;
		private DispatcherTimer DispatcherTimer;

		public SettingsTab()
		{
			InitializeComponent();
			GoBack += ContentPage.GoBack;

			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
			DispatcherTimer = new DispatcherTimer();
			DispatcherTimer.Tick += DispatcherTimer_Tick;
			DispatcherTimer.Interval = TimeSpan.FromSeconds(5);
			DispatcherTimer.Start();
		}

		public override void Dispose()
		{
			base.Dispose();
			ContentPage.Dispose();
			DispatcherTimer.Stop();
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			await Data.CheckForPackages();
			await Data.UpdateShinobuStatus();
			await Data.UpdateServerInfo();
			await Data.CheckForUpdates();
		}

		private async void DispatcherTimer_Tick(object? sender, object e)
		{
			await Data.UpdateShinobuStatus();
			await Data.UpdateServerInfo();
			await Data.CheckThumbnailJob();
		}

	}
}
