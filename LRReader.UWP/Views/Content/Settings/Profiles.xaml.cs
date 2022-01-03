using LRReader.Shared.ViewModels;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{

	public sealed partial class Profiles : Page
	{

		private SettingsPageViewModel Data;

		public Profiles()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}
	}
}
