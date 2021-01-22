using LRReader.Shared.Internal;
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
using LRReader.Shared.Providers;
using Windows.ApplicationModel.Resources;
using Microsoft.Toolkit.Extensions;
using LRReader.UWP.Internal;
using Windows.Services.Store;
using GalaSoft.MvvmLight.Threading;

namespace LRReader.UWP.Views.Main
{

	public sealed partial class LoadingPage : Page
	{
		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		private LoadingPageViewModel ViewModel;

		private ResourceLoader lang;

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
#if !SIDELOAD
			await DownloadUpdate();
#endif
			ViewModel.Active = true;
			await SharedGlobal.UpdatesManager.UpdateSupportedRange(Util.GetAppVersion());

			SharedGlobal.LRRApi.RefreshSettings(SharedGlobal.SettingsManager.Profile);
			var serverInfo = await ServerProvider.GetServerInfo();
			if (serverInfo == null)
			{
				var address = SharedGlobal.SettingsManager.Profile.ServerAddress;
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
			SharedGlobal.ServerInfo = serverInfo;
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
			if (serverInfo.nofun_mode && !SharedGlobal.SettingsManager.Profile.HasApiKey)
			{
				ViewModel.Status = lang.GetString("LoadingPage/MissingKey");
				await Reload();
				return;
			}
			await SharedGlobal.ArchivesManager.ReloadArchives();
			SharedGlobal.ControlFlags.Check(serverInfo);
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

		private async Task DownloadUpdate()
		{
			ViewModel.Active = true;
			var context = StoreContext.GetDefault();

			var packageUpdates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
			if (packageUpdates.Count == 0)
				return;

			if (!context.CanSilentlyDownloadStorePackageUpdates)
				return;
			ViewModel.Active = false;
			await Task.Delay(TimeSpan.FromMilliseconds(500));
			ViewModel.Updating = true;

			var downloadTask = context.TrySilentDownloadStorePackageUpdatesAsync(packageUpdates);

			downloadTask.Progress = async (info, progress) =>
			{
				await DispatcherHelper.RunAsync(() => ViewModel.Progress = progress.TotalDownloadProgress);
			};

			var result = await downloadTask.AsTask();

			ViewModel.Updating = false;

			await Task.Delay(TimeSpan.FromMilliseconds(500));

			switch (result.OverallState)
			{
				case StorePackageUpdateState.Completed:
					await InstallUpdate(packageUpdates, context);
					break;
				default:
					break;
			}
		}

		private async Task InstallUpdate(IReadOnlyList<StorePackageUpdate> packages, StoreContext context)
		{
			ViewModel.Progress = 0;
			ViewModel.Updating = true;

			var installTask = context.TrySilentDownloadAndInstallStorePackageUpdatesAsync(packages);

			installTask.Progress = async (info, progress) =>
			{
				await DispatcherHelper.RunAsync(() => ViewModel.Progress = progress.TotalDownloadProgress);
			};

			var result = await installTask.AsTask();
			ViewModel.Updating = false;
		}
	}
}
