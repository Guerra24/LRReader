using LRReader.Shared.Extensions;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Main : ModernBasePage
	{

		private SettingsPageViewModel Data;

		private ResourceLoader lang;

		public Main()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
			lang = ResourceLoader.GetForCurrentView("Settings");
		}

		private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// This event is getting called multiple times because the frame is not disposed
			// causing the event to not unhook and every time a new settings tab is opened a
			// new event is hooked causing issues due to duplicated page loading
			var profile = e.AddedItems.FirstOrDefault() as ServerProfile;
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
}
