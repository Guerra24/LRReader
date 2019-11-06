using LRReader.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
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

		private async void UserControl_Loaded(object sender, RoutedEventArgs e) => await Data.UpdateShinobuStatus();


		private async void DispatcherTimer_Tick(object sender, object e) => await Data.UpdateShinobuStatus();

		public void RemoveTimer() => DispatcherTimer.Stop();
	}
}
