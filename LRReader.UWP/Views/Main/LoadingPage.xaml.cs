using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Main
{

	public sealed partial class LoadingPage : Page
	{
		private CoreApplicationView CoreView;

		private LoadingPageViewModel ViewModel;

		private static SplashScreen splashScreen = null!;

		public LoadingPage()
		{
			this.InitializeComponent();
			CoreView = CoreApplication.GetCurrentView();
			ViewModel = (LoadingPageViewModel)DataContext;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.Parameter is SplashScreen splash)
				splashScreen = splash;
			UpdateSplash();
			ConnectedAnimationService.GetForCurrentView().GetAnimation("Splash")?.TryStart(Splash);
			TitleBar.Height = CoreView.TitleBar.Height;
			CoreView.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			CoreView.TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
			if (ViewModel.Animate)
			{
				var logo = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Splash", Splash);
				logo.Configuration = new BasicConnectedAnimationConfiguration();
			}
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
			var dpi = DisplayInformation.GetForCurrentView();
			var threshold = dpi.ScreenWidthInRawPixels * 2 / dpi.RawPixelsPerViewPixel - 10;
			if (Service.Platform.DualScreen && ActualWidth > threshold)
			{
				Splash.SetValue(Canvas.LeftProperty, 0);
				Splash.SetValue(Canvas.TopProperty, splashScreen.ImageLocation.Y);
				Splash.Width = ActualWidth / 2;
				Splash.Height = splashScreen.ImageLocation.Height;
			}
			else
			{
				Splash.SetValue(Canvas.LeftProperty, splashScreen.ImageLocation.X);
				Splash.SetValue(Canvas.TopProperty, splashScreen.ImageLocation.Y);
				Splash.Width = splashScreen.ImageLocation.Width;
				Splash.Height = splashScreen.ImageLocation.Height;
			}
		}

	}
}
