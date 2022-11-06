#nullable enable
using LRReader.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Main
{
	public sealed partial class FirstRunPage : Page
	{

		private SettingsPageViewModel Data;

		public FirstRunPage()
		{
			this.InitializeComponent();
			Data = (SettingsPageViewModel)DataContext;
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);

			var logo = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Splash", Splash);
			logo.Configuration = new BasicConnectedAnimationConfiguration();
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			ConnectedAnimationService.GetForCurrentView().GetAnimation("Splash")?.TryStart(Splash);
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Splash.SetValue(Canvas.LeftProperty, e.NewSize.Width / 2d - Splash.ActualWidth / 2d - 67);
			Splash.SetValue(Canvas.TopProperty, e.NewSize.Height / 2d - Splash.ActualHeight / 2d - 58);
		}

	}
}
