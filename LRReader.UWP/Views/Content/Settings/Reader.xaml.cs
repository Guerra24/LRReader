using LRReader.UWP.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Reader : Page
	{
		private SettingsPageViewModel Data;

		public Reader()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;

			var lang = ResourceLoader.GetForCurrentView("Settings");
			ReminderModeRadio.Items.Add(lang.GetString("Reader/ReminderMode/All"));
			ReminderModeRadio.Items.Add(lang.GetString("Reader/ReminderMode/New"));
			/*
				<x:String>All archives</x:String>
				<x:String>Only "New" Archives</x:String>
			 */
		}

	}
}
