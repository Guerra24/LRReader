using System;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class SettingsTab : ModernTab
	{

		private SettingsPageViewModel Data;
		private DispatcherTimer DispatcherTimer;

		public SettingsTab()
		{
			this.InitializeComponent();
			GoBack += () => ContentPage.GoBack();

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

		private async void DispatcherTimer_Tick(object sender, object e)
		{
			await Data.UpdateShinobuStatus();
			await Data.UpdateServerInfo();
			await Data.CheckThumbnailJob();
		}

	}
}
