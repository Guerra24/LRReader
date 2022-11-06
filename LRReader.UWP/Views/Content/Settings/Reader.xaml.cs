using LRReader.Shared.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
			Data = (SettingsPageViewModel)DataContext;

			var lang = ResourceLoader.GetForCurrentView("Settings");
			ReminderModeRadio.Items.Add(lang.GetString("Reader/ReminderMode/All"));
			ReminderModeRadio.Items.Add(lang.GetString("Reader/ReminderMode/New"));
			/*
				<x:String>All archives</x:String>
				<x:String>Only "New" Archives</x:String>
			 */
		}

		private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
		{
			((SolidColorBrush)Application.Current.Resources["CustomReaderBackground"]).Color = args.NewColor;
		}
	}
}
