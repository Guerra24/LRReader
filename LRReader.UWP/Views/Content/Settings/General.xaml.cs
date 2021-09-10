using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.AppCenter.Crashes;
using System;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class General : ModernBasePage
	{
		private SettingsPageViewModel Data;

		private ResourceLoader lang;

		public General()
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

		private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var profile = e.AddedItems.FirstOrDefault() as ServerProfile;
			if (profile == null)
			{
				Service.Tabs.CloseAllTabs();
				Service.Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
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
				Service.Platform.GoToPage(Pages.Loading, PagesTransition.DrillIn);
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
	}
}
