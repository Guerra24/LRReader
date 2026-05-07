using Avalonia.Interactivity;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Content.Settings;

public partial class General : ModernBasePage
{
	public SettingsPageViewModel Data { get; }

	private ResourceLoader lang;

	public General()
	{
		InitializeComponent();

		DataContext = Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
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

		ArchiveStyle.Items.Add(lang.GetString("General/ArchiveStyle/Default"));
		ArchiveStyle.Items.Add(lang.GetString("General/ArchiveStyle/ThumbnailOnly"));
		ArchiveStyle.Items.Add(lang.GetString("General/ArchiveStyle/Compact"));
	}

	private async void ModernBasePage_Loaded(object sender, RoutedEventArgs e)
	{
		VerticalTabs.IsCheckedChanged += ToggleSwitch_Toggled;
		CrashReport.IsCheckedChanged += TrackCrashes_Toggled;
		IncrementalCaching.IsCheckedChanged += IncrementalCaching_Toggled;
		await Data.UpdateThumbnailCacheSize();
	}

	private void ModernBasePage_Unloaded(object sender, RoutedEventArgs e)
	{
		VerticalTabs.IsCheckedChanged -= ToggleSwitch_Toggled;
		CrashReport.IsCheckedChanged -= TrackCrashes_Toggled;
		IncrementalCaching.IsCheckedChanged -= IncrementalCaching_Toggled;
	}

	private void ClearButton_Click(object sender, RoutedEventArgs e)
	{
		Data.SortByIndex = -1;
	}

	private async void TrackCrashes_Toggled(object? sender, RoutedEventArgs e)
	{
		var result = await Service.Platform.OpenGenericDialog(
			Service.Platform.GetLocalizedString("Settings/General/CrashRestartDialog/Title"),
			Service.Platform.GetLocalizedString("Settings/General/CrashRestartDialog/PrimaryButtonText"),
			closebutton: Service.Platform.GetLocalizedString("Settings/General/CrashRestartDialog/CloseButtonText"),
			content: Service.Platform.GetLocalizedString("Settings/General/CrashRestartDialog/Content"));
		if (result == Shared.Models.IDialogResult.Primary)
		{
			/*var res = await CoreApplication.RequestRestartAsync("");
			if (res == AppRestartFailureReason.NotInForeground || res == AppRestartFailureReason.Other)
				WeakReferenceMessenger.Default.Send(new ShowNotification(Service.Platform.GetLocalizedString("Settings/General/UnableRestart/Title"), Service.Platform.GetLocalizedString("Settings/General/UnableRestart/Content"), 0, NotificationSeverity.Error));*/
		}
	}

	private async void ToggleSwitch_Toggled(object? sender, RoutedEventArgs e)
	{
		var result = await Service.Platform.OpenGenericDialog(
			Service.Platform.GetLocalizedString("Settings/General/VerticalTabsDialog/Title"),
			Service.Platform.GetLocalizedString("Settings/General/VerticalTabsDialog/PrimaryButtonText"),
			closebutton: Service.Platform.GetLocalizedString("Settings/General/VerticalTabsDialog/CloseButtonText"),
			content: Service.Platform.GetLocalizedString("Settings/General/VerticalTabsDialog/Content"));
		if (result == Shared.Models.IDialogResult.Primary)
		{
			/*var res = await CoreApplication.RequestRestartAsync("");
			if (res == AppRestartFailureReason.NotInForeground || res == AppRestartFailureReason.Other)
				WeakReferenceMessenger.Default.Send(new ShowNotification(Service.Platform.GetLocalizedString("Settings/General/UnableRestart/Title"), Service.Platform.GetLocalizedString("Settings/General/UnableRestart/Content"), 0, NotificationSeverity.Error));*/
		}
	}

	private async void IncrementalCaching_Toggled(object? sender, RoutedEventArgs e)
	{
		var state = (ToggleSwitch)sender!;
		if (state.IsChecked == Service.Settings.UseIncrementalCaching)
			return;
		Service.Settings.UseIncrementalCaching = !Service.Settings.UseIncrementalCaching;
		var result = await Service.Platform.OpenGenericDialog(
			Service.Platform.GetLocalizedString("Settings/General/SwitchCacheModeDialog/Title"),
			Service.Platform.GetLocalizedString("Settings/General/SwitchCacheModeDialog/PrimaryButtonText"),
			closebutton: Service.Platform.GetLocalizedString("Settings/General/SwitchCacheModeDialog/CloseButtonText"),
			content: Service.Platform.GetLocalizedString("Settings/General/SwitchCacheModeDialog/Content"));
		if (result == Shared.Models.IDialogResult.Primary)
		{
			await Service.Session.Suspend();
			Service.Tabs.CloseAllTabs();
			Service.Platform.GoToPage(Pages.Loading, PagesTransition.DrillIn);
		}
		else
		{
			Service.Settings.UseIncrementalCaching = !Service.Settings.UseIncrementalCaching;
		}
	}
}