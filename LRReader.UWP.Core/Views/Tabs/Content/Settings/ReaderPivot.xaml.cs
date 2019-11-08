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

namespace LRReader.Views.Tabs.Content.Settings
{
	public sealed partial class ReaderPivot : PivotItem
	{
		private SettingsPageViewModel Data;

		public ReaderPivot()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e) => await Data.UpdateCacheSize();

		private async void ButtonClearCache_Click(object sender, RoutedEventArgs e)
		{
			await Data.ClearCache();
			await Data.UpdateCacheSize();
		}
	}
}
