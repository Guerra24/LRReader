using Avalonia.Interactivity;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Extensions;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;

namespace LRReader.UWP.Views.Content.Settings;

public partial class Main : ModernBasePage
{
	private SettingsPageViewModel Data;

	private ResourceLoader lang;

	public Main()
	{
		InitializeComponent();
		Data = (SettingsPageViewModel)DataContext!;
		lang = ResourceLoader.GetForCurrentView("Settings");
	}

	private async void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		// This event is getting called multiple times because the frame is not disposed
		// causing the event to not unhook and every time a new settings tab is opened a
		// new event is hooked causing issues due to duplicated page loading
		var profile = e.AddedItems.Cast<ServerProfile>().FirstOrDefault();
		if (profile == null)
		{
			Service.Tabs.CloseAllTabs();
			Service.Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
			return;
		}
		if (profile == Data.SettingsManager.Profile)
			return;

		var result = await Service.Platform.OpenGenericDialog(lang.GetString("General/SwitchProfile/Title"), lang.GetString("General/SwitchProfile/PrimaryButtonText"), closebutton: lang.GetString("General/SwitchProfile/CloseButtonText"), content: lang.GetString("General/SwitchProfile/Content").AsFormat("\n"));
		if (result == IDialogResult.Primary)
		{
			await Service.Session.Suspend();
			Data.SettingsManager.Profile = profile;
			Service.Tabs.CloseAllTabs();
			Service.Platform.GoToPage(Pages.Loading, PagesTransition.DrillIn);
		}
		else
		{
			ProfileSelection.SelectedItem = Data.SettingsManager.Profile;
		}
	}

	private void ModernBasePage_Loaded(object sender, RoutedEventArgs e) => ProfileSelection.SelectionChanged += ComboBox_SelectionChanged;

	private void ModernBasePage_Unloaded(object sender, RoutedEventArgs e) => ProfileSelection.SelectionChanged -= ComboBox_SelectionChanged;
}