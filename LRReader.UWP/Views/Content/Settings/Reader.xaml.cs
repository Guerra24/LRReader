using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WinRT;
using ColorChangedEventArgs = Microsoft.UI.Xaml.Controls.ColorChangedEventArgs;
using ColorPicker = Microsoft.UI.Xaml.Controls.ColorPicker;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Reader : Page
	{
		private SettingsPageViewModel Data;

		public Reader()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();

			var lang = ResourceLoader.GetForCurrentView("Settings");
			ReminderModeRadio.Items.Add(lang.GetString("Reader/ReminderMode/All"));
			ReminderModeRadio.Items.Add(lang.GetString("Reader/ReminderMode/New"));
			/*
				<x:String>All archives</x:String>
				<x:String>Only "New" Archives</x:String>
			 */
			ClearNew.Items.Add(lang.GetString("Reader/ClearNew/Original"));
			ClearNew.Items.Add(lang.GetString("Reader/ClearNew/Web"));
			ClearNew.Items.Add(lang.GetString("Reader/ClearNew/Custom"));
		}

		[DynamicWindowsRuntimeCast(typeof(SolidColorBrush))]
		private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
		{
			((SolidColorBrush)Application.Current.Resources["CustomReaderBackground"]).Color = args.NewColor;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await Data.RefreshCategories();
		}

		[DynamicWindowsRuntimeCast(typeof(ToggleSwitch))]
		private void SyncBookmarks_Toggled(object sender, RoutedEventArgs e)
		{
			var toggleSwitch = (ToggleSwitch)sender;
			Data.SettingsManager.Profile.SynchronizeBookmarks = toggleSwitch.IsOn;
			Data.SettingsManager.SaveProfiles();
		}
	}
}
