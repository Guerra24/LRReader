using LRReader.Shared.Internal;
using static LRReader.Shared.Providers.Providers;
using LRReader.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using LRReader.Internal;

namespace LRReader.UWP.Views.Main
{

	public sealed partial class LoadingPage : Page
	{
		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		private LoadingPageViewModel ViewModel;

		public LoadingPage()
		{
			this.InitializeComponent();
			CoreView = CoreApplication.GetCurrentView();
			AppView = ApplicationView.GetForCurrentView();
			ViewModel = DataContext as LoadingPageViewModel;
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

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			TitleBar.Height = coreTitleBar.Height;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			ViewModel.Active = true;
			await SharedGlobal.UpdatesManager.UpdateSupportedRange(Util.GetAppVersion());

			SharedGlobal.LRRApi.RefreshSettings(SharedGlobal.SettingsManager.Profile);
			var serverInfo = await ServerProvider.GetServerInfo();
			if (serverInfo == null)
			{
				ViewModel.Status = "Unable to connect... Please select another profile";
				await Reload();
				return;
			}
			else if (serverInfo._unauthorized)
			{
				ViewModel.Status = "Invalid or missing API key";
				await Reload();
				return;
			}
			SharedGlobal.ServerInfo = serverInfo;
			if (serverInfo.version < UpdatesManager.MIN_VERSION)
			{
				ViewModel.Status = $"Version {serverInfo.version} not supported. Please update your LANraragi instance to {UpdatesManager.MIN_VERSION}";
				await Reload();
				return;
			}
			else if (serverInfo.version > UpdatesManager.MAX_VERSION)
			{
				ViewModel.Status = $"Version {serverInfo.version} not supported. Please update the app to a newer version";
				ViewModel.StatusSub = $"LANraragi supported range {UpdatesManager.MIN_VERSION} - {UpdatesManager.MAX_VERSION}";
				await Reload();
				return;
			}
			if (serverInfo.nofun_mode && !SharedGlobal.SettingsManager.Profile.HasApiKey)
			{
				ViewModel.Status = "Running in No-Fun mode, API key required";
				await Reload();
				return;
			}
			await SharedGlobal.ArchivesManager.ReloadArchives();
			ViewModel.Active = false;
			(Window.Current.Content as Frame).Navigate(typeof(HostTabPage), null, new DrillInNavigationTransitionInfo());
		}

		private async Task Reload()
		{
			ViewModel.Active = false;
			await Task.Delay(TimeSpan.FromSeconds(5));
			ViewModel.Status = "";
			ViewModel.StatusSub = "";
			(Window.Current.Content as Frame).Navigate(typeof(FirstRunPage), null, new DrillInNavigationTransitionInfo());
		}
	}
}
