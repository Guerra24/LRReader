﻿using LRReader.Shared.Extensions;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public partial class LoadingPageViewModel : ObservableObject
	{
		private readonly SettingsService Settings;
		private readonly PlatformService Platform;
		private readonly ApiService Api;
		private readonly ArchivesService Archives;
		private readonly UpdatesService Updates;
		private readonly ISettingsStorageService SettingsStorage;

		[ObservableProperty]
		private string _status = "";
		[ObservableProperty]
		private string _statusSub = "";
		[ObservableProperty]
		private bool _active;
		[ObservableProperty]
		private bool _updating;
		[ObservableProperty]
		private double _progress;
		[ObservableProperty]
		private bool _retry;
		[ObservableProperty]
		private bool _animate;

		public LoadingPageViewModel(SettingsService settings, PlatformService platform, ApiService api, ArchivesService archives, UpdatesService updates, ISettingsStorageService settingsStorage)
		{
			Settings = settings;
			Platform = platform;
			Api = api;
			Archives = archives;
			Updates = updates;
			SettingsStorage = settingsStorage;
		}

		private async Task Reload(double time = 5)
		{
			Active = false;
			Animate = true;
			await Task.Delay(TimeSpan.FromSeconds(time));
			Status = "";
			StatusSub = "";
			Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
		}

		public async Task Startup()
		{
			Animate = false;
			await Service.InitServices();
			if (Updates.CanAutoUpdate() && Settings.AutoUpdate)
			{
				var update = await Updates.CheckForUpdates();
				if (update.Result)
				{
					Updating = true;
					// Set wasUpdate in settingstorage
					SettingsStorage.StoreObjectLocal("WasUpdated", true);
					var result = await Updates.DownloadAndInstall(new Progress<double>(progress => Progress = progress), update);
					Updating = false;
					if (!result.Result)
					{
						SettingsStorage.DeleteObjectLocal("WasUpdated");
						if (result.ErrorMessage != null)
							Status = result.ErrorMessage;
						StatusSub = Platform.GetLocalizedString("Pages/LoadingPage/UpdateErrorCode").AsFormat(result.ErrorCode);
						await Task.Delay(TimeSpan.FromSeconds(3));
						Status = "";
						StatusSub = "";
					}
				}
			}

			bool firstRun = Settings.Profile == null || (!Settings.AutoLogin && Settings.FirstStartup);
			if (firstRun)
			{
				Animate = true;
				await Task.Delay(TimeSpan.FromMilliseconds(500));
				Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
				return;
			}
			Settings.FirstStartup = false;

			Active = true;
#if !DEBUG
			await Updates.UpdateSupportedRange();
#endif
			await Connect();
		}

		[ICommand]
		private async Task Connect()
		{
			Status = "";
			StatusSub = "";
			Retry = false;
			Active = true;
			if (!Api.RefreshSettings(Settings.Profile))
			{
				Status = Platform.GetLocalizedString("Pages/LoadingPage/InvalidAddress");
				StatusSub = Platform.GetLocalizedString("Pages/LoadingPage/InvalidAddressSub");
				await Reload();
				return;
			}
			var serverInfo = await ServerProvider.GetServerInfo();
			if (serverInfo == null)
			{
				var address = Settings.Profile.ServerAddress;
				if (address.Contains("127.0.0.") || address.Contains("localhost"))
				{
					Status = Platform.GetLocalizedString("Pages/LoadingPage/NoConnectionLocalHost");
					StatusSub = Platform.GetLocalizedString("Pages/LoadingPage/NoConnectionLocalHostSub");
				}
				else
					Status = Platform.GetLocalizedString("Pages/LoadingPage/NoConnection");
				Active = false;
				await Task.Delay(TimeSpan.FromSeconds(2.5));
				Retry = true;
				return;
			}
			else if (serverInfo._unauthorized)
			{
				Status = Platform.GetLocalizedString("Pages/LoadingPage/InvalidKey");
				await Reload();
				return;
			}
			if (serverInfo.version < Updates.MIN_VERSION)
			{
				Status = Platform.GetLocalizedString("Pages/LoadingPage/InstanceNotSupported").AsFormat(serverInfo.version, Updates.MIN_VERSION);
				await Reload();
				return;
			}
			else if (serverInfo.version > Updates.MAX_VERSION)
			{
				Status = Platform.GetLocalizedString("Pages/LoadingPage/ClientNotSupported").AsFormat(serverInfo.version);
				StatusSub = Platform.GetLocalizedString("Pages/LoadingPage/ClientRange").AsFormat(Updates.MIN_VERSION, Updates.MAX_VERSION);
				await Reload();
				return;
			}

			if (!await Api.Validate())
			{
				await Reload(0.5);
				return;
			}

			Api.ServerInfo = serverInfo;
			await Archives.ReloadArchives();
			Api.ControlFlags.Check(serverInfo);
			Active = false;
			Platform.GoToPage(Pages.HostTab, PagesTransition.DrillIn);
		}

		[ICommand]
		private void Change()
		{
			Status = "";
			StatusSub = "";
			Retry = false;
			Active = false;
			Animate = true;
			Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
		}
	}
}
