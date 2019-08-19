using LRReader.Internal;
using LRReader.ViewModels;
using LRReader.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views.Main
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SettingsPage : Page
	{
		private SettingsPageViewModel Data;

		public SettingsPage()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			ButtonClearCache.IsEnabled = false;
			RingCacheClear.IsActive = true;
			await Data.UpdateCacheSize();
			RingCacheClear.IsActive = false;
			ButtonClearCache.IsEnabled = true;
		}

		private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			ServerProfileDialog dialog = new ServerProfileDialog(false);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.AddProfile(dialog.ProfileName.Text, dialog.ProfileServerAddress.Text, dialog.ProfileServerApiKey.Password);
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
				Global.LRRApi.RefreshSettings();
			}
		}

		private async void ButtonRemove_Click(object sender, RoutedEventArgs e)
		{
			ContentDialog dialog = new ContentDialog { Title = "Remove Profile?", PrimaryButtonText = "Yes", CloseButtonText = "No" };
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				var sm = Data.SettingsManager;
				sm.Profiles.Remove(sm.Profile);
				sm.Profile = null;
				sm.Profile = sm.Profiles.First();
			}
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Data.SettingsManager.Profile != null)
				Global.LRRApi.RefreshSettings();
		}

		private async void ButtonClearCache_Click(object sender, RoutedEventArgs e)
		{
			ButtonClearCache.IsEnabled = false;
			RingCacheClear.IsActive = true;
			await Global.ImageManager.ClearCache();
			await Data.UpdateCacheSize();
			RingCacheClear.IsActive = false;
			ButtonClearCache.IsEnabled = true;
		}
	}
}
