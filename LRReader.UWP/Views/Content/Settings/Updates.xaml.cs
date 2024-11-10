using LRReader.Shared.ViewModels;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Updates : Page
	{
		private SettingsPageViewModel Data;

		public Updates()
		{
			this.InitializeComponent();
			Data = (SettingsPageViewModel)DataContext;
		}

	}
}
