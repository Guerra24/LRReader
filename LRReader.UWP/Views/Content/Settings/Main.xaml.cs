using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using System.Linq;
using Windows.UI.Xaml.Controls;
using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using Windows.ApplicationModel.Resources;
using LRReader.Shared.Extensions;
using System;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Main : ModernBasePage
	{

		private SettingsPageViewModel Data;

		private ResourceLoader lang;

		public Main()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
			lang = ResourceLoader.GetForCurrentView("Settings");
		}

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

	}
}
