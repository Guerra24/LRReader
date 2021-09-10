using LRReader.UWP.ViewModels;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Main
{
	public sealed partial class FirstRunPage : Page
	{

		private SettingsPageViewModel Data;

		public FirstRunPage()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

	}
}
