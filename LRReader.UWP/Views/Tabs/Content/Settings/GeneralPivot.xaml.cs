using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Main;
using Microsoft.AppCenter.Crashes;
using System;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Views.Tabs.Content.Settings
{
	public sealed partial class GeneralPivot : PivotItem
	{
		private SettingsPageViewModel Data;

		private ResourceLoader lang;

		public GeneralPivot()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
			lang = ResourceLoader.GetForCurrentView("Settings");
			ThemeComboBox.Items.Add(lang.GetString("General/AppTheme/System"));
			ThemeComboBox.Items.Add(lang.GetString("General/AppTheme/Dark"));
			ThemeComboBox.Items.Add(lang.GetString("General/AppTheme/Light"));
			/*
				<x:String>System</x:String>
				<x:String>Dark</x:String>
				<x:String>Light</x:String>
			 */
			var langb = ResourceLoader.GetForCurrentView("Tabs");
			OrderByComboBox.Items.Add(langb.GetString("Archives/OrderAsc/Text"));
			OrderByComboBox.Items.Add(langb.GetString("Archives/OrderDesc/Text"));
		}

		private async void PivotItem_Loaded(object sender, RoutedEventArgs e) => await Data.UpdateThumbnailCacheSize();

		private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ServerProfileDialog(false);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				var url = dialog.ProfileServerAddress.Text;
				if (!(url.StartsWith("http://") || url.StartsWith("https://")))
					url = "http://" + url;
				Data.SettingsManager.AddProfile(dialog.ProfileName.Text, url, dialog.ProfileServerApiKey.Password);
			}
		}

		private async void ButtonEdit_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ServerProfileDialog(true);
			var profile = Data.SettingsManager.Profile;
			dialog.ProfileName.Text = profile.Name;
			dialog.ProfileServerAddress.Text = profile.ServerAddress;
			dialog.ProfileServerApiKey.Password = profile.ServerApiKey;

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				var url = dialog.ProfileServerAddress.Text;
				if (!(url.StartsWith("http://") || url.StartsWith("https://")))
					url = "http://" + url;
				Data.SettingsManager.ModifyProfile(profile.UID, dialog.ProfileName.Text, url, dialog.ProfileServerApiKey.Password);
				Service.Api.RefreshSettings(Data.SettingsManager.Profile);
			}
		}

		private void ButtonRemove_Click(object sender, RoutedEventArgs e)
		{
			var sm = Data.SettingsManager;
			sm.Profiles.Remove(sm.Profile);
			sm.Profile = sm.Profiles.FirstOrDefault();
			ProfileSelection.SelectedItem = sm.Profile;
			RemoveFlyout.Hide();
		}

		private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var profile = e.AddedItems.FirstOrDefault() as ServerProfile;
			if (profile == null)
			{
				Service.Tabs.CloseAllTabs();
				(Window.Current.Content as Root).Frame.Navigate(typeof(FirstRunPage), null, new DrillInNavigationTransitionInfo());
				return;
			}
			if (profile == Data.SettingsManager.Profile)
				return;
			var dialog = new ContentDialog()
			{
				Title = lang.GetString("General/SwitchProfile/Title"),
				Content = lang.GetString("General/SwitchProfile/Content").AsFormat("\n"),
				PrimaryButtonText = lang.GetString("General/SwitchProfile/PrimaryButtonText"),
				CloseButtonText = lang.GetString("General/SwitchProfile/CloseButtonText")
			};
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.Profile = profile;
				Service.Tabs.CloseAllTabs();
				(Window.Current.Content as Root).Frame.Navigate(typeof(LoadingPage), null, new DrillInNavigationTransitionInfo());
			}
			else
			{
				ProfileSelection.SelectedItem = Data.SettingsManager.Profile;
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.SortByIndex = -1;
		}

		private async void TrackCrashes_Toggled(object sender, RoutedEventArgs e)
		{
			await Crashes.SetEnabledAsync((sender as ToggleSwitch).IsOn);
		}

		private async void TrackCrashes_Loading(FrameworkElement sender, object args)
		{
			(sender as ToggleSwitch).IsOn = await Crashes.IsEnabledAsync();
		}

		private async void ButtonClearThumbCache_Click(object sender, RoutedEventArgs e) => await Data.ClearThumbnailCache();

	}
}
