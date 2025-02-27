using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.ApplicationModel.Core;
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
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
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


		private async void ModernBasePage_Loaded(object sender, RoutedEventArgs e)
		{
			VerticalTabs.Toggled += ToggleSwitch_Toggled;
			CrashReport.Toggled += TrackCrashes_Toggled;
			IncrementalCaching.Toggled += IncrementalCaching_Toggled;
			await Data.UpdateThumbnailCacheSize();
		}

		private void ModernBasePage_Unloaded(object sender, RoutedEventArgs e)
		{
			VerticalTabs.Toggled -= ToggleSwitch_Toggled;
			CrashReport.Toggled -= TrackCrashes_Toggled;
			IncrementalCaching.Toggled -= IncrementalCaching_Toggled;
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.SortByIndex = -1;
		}

		private async void TrackCrashes_Toggled(object sender, RoutedEventArgs e)
		{
			var result = await Service.Platform.OpenGenericDialog("Toggle error and crash logs reporting?", "Yes, restart", closebutton: "Restart later", content: "The app needs to restart to apply changes");
			if (result == Shared.Models.IDialogResult.Primary)
			{
				var res = await CoreApplication.RequestRestartAsync("");
				if (res == AppRestartFailureReason.NotInForeground || res == AppRestartFailureReason.Other)
					WeakReferenceMessenger.Default.Send(new ShowNotification("Unable to restart application", "Please restart manually.", 0, NotificationSeverity.Error));
			}
		}

		private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			var result = await Service.Platform.OpenGenericDialog("Switch tabs mode?", "Yes, restart", closebutton: "Restart later", content: "The app needs to restart to switch tabs mode");
			if (result == Shared.Models.IDialogResult.Primary)
			{
				var res = await CoreApplication.RequestRestartAsync("");
				if (res == AppRestartFailureReason.NotInForeground || res == AppRestartFailureReason.Other)
					WeakReferenceMessenger.Default.Send(new ShowNotification("Unable to restart application", "Please restart manually.", 0, NotificationSeverity.Error));
			}
		}

		private async void IncrementalCaching_Toggled(object? sender, RoutedEventArgs e)
		{
			var state = (ToggleSwitch)sender!;
			if (state.IsOn == Service.Settings.UseIncrementalCaching)
				return;
			Service.Settings.UseIncrementalCaching = !Service.Settings.UseIncrementalCaching;
			var result = await Service.Platform.OpenGenericDialog("Switch caching mode?", "Yes, reload profile", closebutton: "Cancel", content: "The app needs to reload the profile to switch caching mode");
			if (result == Shared.Models.IDialogResult.Primary)
			{
				Service.Tabs.CloseAllTabs();
				Service.Platform.GoToPage(Pages.Loading, PagesTransition.DrillIn);
			}
			else
			{
				Service.Settings.UseIncrementalCaching = !Service.Settings.UseIncrementalCaching;
			}
		}

	}
}
