using LRReader.Internal;
using LRReader.Models.Main;
using LRReader.ViewModels;
using LRReader.Views.Dialogs;
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
	public sealed partial class FirstRunTabContent : UserControl
	{
		private SettingsPageViewModel Data;

		public FirstRunTabContent()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			ServerProfileDialog dialog = new ServerProfileDialog(false);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.Profile = Data.SettingsManager.AddProfile(dialog.ProfileName.Text, dialog.ProfileServerAddress.Text, dialog.ProfileServerApiKey.Password);
			}
		}

		private async void ButtonEdit_Click(object sender, RoutedEventArgs e)
		{
			ServerProfileDialog dialog = new ServerProfileDialog(true);
			ServerProfile profile = Data.SettingsManager.Profile;
			dialog.ProfileName.Text = profile.Name;
			dialog.ProfileServerAddress.Text = profile.ServerAddress;
			dialog.ProfileServerApiKey.Password = profile.ServerApiKey;

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.ModifyProfile(profile.UID, dialog.ProfileName.Text, dialog.ProfileServerAddress.Text, dialog.ProfileServerApiKey.Password);
			}
		}

		private void ButtonContinue_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.CloseAllTabs();
			Global.EventManager.AddTab(new ArchivesTab());
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Global.LRRApi.RefreshSettings();
		}
	}
}
