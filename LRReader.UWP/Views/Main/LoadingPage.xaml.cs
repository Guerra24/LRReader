using LRReader.Shared.Extensions;
using LRReader.Shared.Internal;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.AppCenter.Crashes;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Services.Store;
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
#if !SIDELOAD && !DEBUG
			await DownloadUpdateStore();
#endif

			bool firstRun = Settings.Profile == null;
			if (firstRun)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(500));
				(Window.Current.Content as Root).Frame.Navigate(typeof(FirstRunPage), null, new DrillInNavigationTransitionInfo());
				return;
			}

			ViewModel.Active = true;
#if !DEBUG
			await SharedGlobal.UpdatesManager.UpdateSupportedRange(Platform.Version);
#endif
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
				await Reload();
				return;
			}
			else if (serverInfo._unauthorized)
			{
				ViewModel.Status = lang.GetString("LoadingPage/InvalidKey");
				await Reload();
				return;
			}
			Api.ServerInfo = serverInfo;
			if (serverInfo.version < UpdatesManager.MIN_VERSION)
			{
				ViewModel.Status = lang.GetString("LoadingPage/InstanceNotSupported").AsFormat(serverInfo.version, UpdatesManager.MIN_VERSION);
				await Reload();
				return;
			}
			else if (serverInfo.version > UpdatesManager.MAX_VERSION)
			{
				ViewModel.Status = lang.GetString("LoadingPage/ClientNotSupported").AsFormat(serverInfo.version);
				ViewModel.StatusSub = lang.GetString("LoadingPage/ClientRange").AsFormat(UpdatesManager.MIN_VERSION, UpdatesManager.MAX_VERSION);
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

		private async Task Reload()
		{
			ViewModel.Active = false;
			await Task.Delay(TimeSpan.FromSeconds(5));
			ViewModel.Status = "";
			ViewModel.StatusSub = "";
			(Window.Current.Content as Root).Frame.Navigate(typeof(FirstRunPage), null, new DrillInNavigationTransitionInfo());
		}

		private async Task DownloadUpdateStore()
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
				return;

			var context = StoreContext.GetDefault();
			if (!context.CanSilentlyDownloadStorePackageUpdates)
				return;
			try
			{
				var packageUpdates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
				if (packageUpdates.Count == 0)
					return;

				ViewModel.Updating = true;

				var downloadTask = context.TrySilentDownloadAndInstallStorePackageUpdatesAsync(packageUpdates);
				//downloadTask = context.RequestDownloadAndInstallStorePackageUpdatesAsync(packageUpdates);

				downloadTask.Progress = async (info, progress) =>
				{
					await Service.Dispatcher.RunAsync(() => ViewModel.Progress = progress.TotalDownloadProgress);
				};
				var result = await downloadTask.AsTask();
			}
			catch (Exception e)
			{
				Crashes.TrackError(e);

				ViewModel.Updating = false;

				ViewModel.Status = lang.GetString("LoadingPage/UpdateError");
				ViewModel.StatusSub = lang.GetString("LoadingPage/UpdateErrorCode").AsFormat(e.HResult);

				await Task.Delay(TimeSpan.FromSeconds(3));

				ViewModel.Status = "";
				ViewModel.StatusSub = "";
			}
			finally
			{
				ViewModel.Updating = false;
			}
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
