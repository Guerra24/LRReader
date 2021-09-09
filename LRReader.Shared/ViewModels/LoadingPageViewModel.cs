using LRReader.Shared.Extensions;
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

		public LoadingPageViewModel(SettingsService settings, PlatformService platform, ApiService api, ArchivesService archives, UpdatesService updates, ISettingsStorageService settingsStorage)
		{
			Settings = settings;
			Platform = platform;
			Api = api;
			Archives = archives;
			Updates = updates;
			SettingsStorage = settingsStorage;
		}

		private async Task Reload()
		{
			Active = false;
			await Task.Delay(TimeSpan.FromSeconds(5));
			Status = "";
			StatusSub = "";
			Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
		}

		public async Task Startup()
		{
			await Service.InitServices();
			if (Updates.CanAutoUpdate() && Updates.AutoUpdate)
			{
				var update = await Updates.CheckForUpdates();
				if (update.Found)
				{
					Updating = true;
					// Set wasUpdate in settingstorage
					SettingsStorage.StoreObjectLocal("WasUpdated", true);
					var result = await Updates.DownloadAndInstall(new Progress<double>(progress => Progress = progress));
					Updating = false;
					if (!result.Result)
					{
						SettingsStorage.DeleteObjectLocal("WasUpdated");
						Status = result.ErrorMessage;
						StatusSub = Platform.GetLocalizedString("Pages/LoadingPage/UpdateErrorCode").AsFormat(result.ErrorCode);
						await Task.Delay(TimeSpan.FromSeconds(3));
						Status = "";
						StatusSub = "";
					}
				}
			}

			bool firstRun = Settings.Profile == null;
			if (firstRun)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(500));
				Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
				return;
			}

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
			Api.RefreshSettings(Settings.Profile);
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
			Api.ServerInfo = serverInfo;
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
			if (serverInfo.nofun_mode && !Settings.Profile.HasApiKey)
			{
				Status = Platform.GetLocalizedString("Pages/LoadingPage/MissingKey");
				await Reload();
				return;
			}
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
			Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
		}
	}
}
