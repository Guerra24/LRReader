using LRReader.Shared.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Main
{

	public sealed partial class LoadingPage : Page
	{
		private CoreApplicationView CoreView;

		private LoadingPageViewModel ViewModel;

		private static SplashScreen splashScreen;

		public LoadingPage()
		{
			this.InitializeComponent();
			CoreView = CoreApplication.GetCurrentView();
			ViewModel = DataContext as LoadingPageViewModel;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.Parameter is SplashScreen splash)
				splashScreen = splash;
			UpdateSplash();
			TitleBar.Height = CoreView.TitleBar.Height;
			CoreView.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			CoreView.TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			TitleBar.Height = coreTitleBar.Height;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await ViewModel.Startup();
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateSplash();
		}

		private void UpdateSplash()
		{
			Splash.SetValue(Canvas.LeftProperty, splashScreen.ImageLocation.X);
			Splash.SetValue(Canvas.TopProperty, splashScreen.ImageLocation.Y);
			Splash.Width = splashScreen.ImageLocation.Width;
			Splash.Height = splashScreen.ImageLocation.Height;
		}

	}
}
