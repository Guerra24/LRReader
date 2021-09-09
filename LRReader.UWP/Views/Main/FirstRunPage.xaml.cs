using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Main
{

	public sealed partial class FirstRunPage : Page
	{
		private CoreApplicationView CoreView;

		private SettingsPageViewModel Data;

		public FirstRunPage()
		{
			this.InitializeComponent();

			CoreView = CoreApplication.GetCurrentView();
			Data = DataContext as SettingsPageViewModel;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			TitleBar.Height = CoreView.TitleBar.Height;
			CoreView.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			CoreView.TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args) => TitleBar.Height = coreTitleBar.Height;

		private void ButtonRemove_Click(object sender, RoutedEventArgs e) => RemoveFlyout.Hide();

		private void ButtonContinue_Click(object sender, RoutedEventArgs e) => Service.Platform.GoToPage(Pages.Loading, PagesTransition.DrillIn);
	}
}
