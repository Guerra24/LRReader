using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.AppCenter.Crashes;
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

			TagsPopupComboBox.Items.Add(lang.GetString("General/PopupLocation/Top"));
			TagsPopupComboBox.Items.Add(lang.GetString("General/PopupLocation/Middle"));
			TagsPopupComboBox.Items.Add(lang.GetString("General/PopupLocation/Bottom"));
		}

		private async void PivotItem_Loaded(object sender, RoutedEventArgs e) => await Data.UpdateThumbnailCacheSize();

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.SortByIndex = -1;
		}

		private async void TrackCrashes_Toggled(object sender, RoutedEventArgs e)
		{
			if (!Data.SettingsManager.Profile.AcceptedDisclaimer)
			{
				var value = (sender as ToggleSwitch).IsOn;
				Data.SettingsManager.CrashReporting = value;
				await Crashes.SetEnabledAsync(value);
			}
		}

		private async void TrackCrashes_Loading(FrameworkElement sender, object args)
		{
			(sender as ToggleSwitch).IsOn = await Crashes.IsEnabledAsync();
		}
	}
}
