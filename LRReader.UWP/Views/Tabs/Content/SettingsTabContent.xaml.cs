using LRReader.UWP.ViewModels;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class SettingsTabContent : UserControl
	{
		private SettingsPageViewModel Data;
		private DispatcherTimer DispatcherTimer;

		public SettingsTabContent()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
			DispatcherTimer = new DispatcherTimer();
			DispatcherTimer.Tick += DispatcherTimer_Tick;
			DispatcherTimer.Interval = new TimeSpan(0, 0, 5);
			DispatcherTimer.Start();
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

		public void RemoveTimer() => DispatcherTimer.Stop();
	}
}
