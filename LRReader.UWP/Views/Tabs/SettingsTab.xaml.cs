using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
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
			DispatcherTimer.Interval = new TimeSpan(0, 0, 5);
			DispatcherTimer.Start();
		}

		public override void Unload()
		{
			base.Unload();
			DispatcherTimer.Stop();
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			await Data.CheckForPackages();
			await Data.UpdateShinobuStatus();
			await Data.UpdateServerInfo();
		}

		private async void DispatcherTimer_Tick(object sender, object e)
		{
			await Data.UpdateShinobuStatus();
			await Data.UpdateServerInfo();
			await Data.CheckThumbnailJob();
		}

	}
}
