using LRReader.Shared.Extensions;
using LRReader.Shared.Providers;
using LRReader.Shared.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.Views.Main
{

	public sealed partial class LoadingPage : Page
	{
		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		private LoadingPageViewModel ViewModel;

		private ResourceLoader lang;

		private static SplashScreen splashScreen;

		public LoadingPage()
		{
			this.InitializeComponent();
			CoreView = CoreApplication.GetCurrentView();
			AppView = ApplicationView.GetForCurrentView();
			ViewModel = DataContext as LoadingPageViewModel;
			lang = ResourceLoader.GetForCurrentView("Pages");
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
			await InitServices();
			if (Updates.CanAutoUpdate() && Updates.AutoUpdate)
			{
				var update = await Updates.CheckForUpdates();
				if (update.Found)
				{
					ViewModel.Updating = true;
					// Set wasUpdate in settingstorage
					SettingsStorage.StoreObjectLocal("WasUpdated", true);
					var result = await Updates.DownloadAndInstall(new Progress<double>(progress => ViewModel.Progress = progress));
					ViewModel.Updating = false;
					if (!result.Result)
					{
						SettingsStorage.DeleteObjectLocal("WasUpdated");
						ViewModel.Status = result.ErrorMessage;
						ViewModel.StatusSub = lang.GetString("LoadingPage/UpdateErrorCode").AsFormat(result.ErrorCode);
						await Task.Delay(TimeSpan.FromSeconds(3));
						ViewModel.Status = "";
						ViewModel.StatusSub = "";
					}
				}
			}

			bool firstRun = Settings.Profile == null;
			if (firstRun)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(500));
				(Window.Current.Content as Root).Frame.Navigate(typeof(FirstRunPage), null, new DrillInNavigationTransitionInfo());
				return;
			}

			ViewModel.Active = true;
#if !DEBUG
			await Updates.UpdateSupportedRange();
#endif
			await Connect();
		}

		private async Task Reload()
		{
			ViewModel.Active = false;
			await Task.Delay(TimeSpan.FromSeconds(5));
			ViewModel.Status = "";
			ViewModel.StatusSub = "";
			(Window.Current.Content as Root).Frame.Navigate(typeof(FirstRunPage), null, new DrillInNavigationTransitionInfo());
		}

		private async Task Connect()
		{
			ViewModel.Status = "";
			ViewModel.StatusSub = "";
			ViewModel.Retry = false;
			ViewModel.Active = true;
			Api.RefreshSettings(Settings.Profile);
			var serverInfo = await ServerProvider.GetServerInfo();
			if (serverInfo == null)
			{
				var address = Settings.Profile.ServerAddress;
				if (address.Contains("127.0.0.") || address.Contains("localhost"))
				{
					ViewModel.Status = lang.GetString("LoadingPage/NoConnectionLocalHost");
					ViewModel.StatusSub = lang.GetString("LoadingPage/NoConnectionLocalHostSub");
				}
				else
					ViewModel.Status = lang.GetString("LoadingPage/NoConnection");
				ViewModel.Active = false;
				await Task.Delay(TimeSpan.FromSeconds(2.5));
				ViewModel.Retry = true;
				return;
			}
			else if (serverInfo._unauthorized)
			{
				ViewModel.Status = lang.GetString("LoadingPage/InvalidKey");
				await Reload();
				return;
			}
			Api.ServerInfo = serverInfo;
			if (serverInfo.version < Updates.MIN_VERSION)
			{
				ViewModel.Status = lang.GetString("LoadingPage/InstanceNotSupported").AsFormat(serverInfo.version, Updates.MIN_VERSION);
				await Reload();
				return;
			}
			else if (serverInfo.version > Updates.MAX_VERSION)
			{
				ViewModel.Status = lang.GetString("LoadingPage/ClientNotSupported").AsFormat(serverInfo.version);
				ViewModel.StatusSub = lang.GetString("LoadingPage/ClientRange").AsFormat(Updates.MIN_VERSION, Updates.MAX_VERSION);
				await Reload();
				return;
			}
			if (serverInfo.nofun_mode && !Settings.Profile.HasApiKey)
			{
				ViewModel.Status = lang.GetString("LoadingPage/MissingKey");
				await Reload();
				return;
			}
			await Archives.ReloadArchives();
			Api.ControlFlags.Check(serverInfo);
			ViewModel.Active = false;
			(Window.Current.Content as Root).Frame.Navigate(typeof(HostTabPage), null, new DrillInNavigationTransitionInfo());
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

		private async void Retry_Click(object sender, RoutedEventArgs e) => await Connect();

		private void Change_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.Status = "";
			ViewModel.StatusSub = "";
			ViewModel.Retry = false;
			ViewModel.Active = false;
			(Window.Current.Content as Root).Frame.Navigate(typeof(FirstRunPage), null, new DrillInNavigationTransitionInfo());
		}
	}
}
