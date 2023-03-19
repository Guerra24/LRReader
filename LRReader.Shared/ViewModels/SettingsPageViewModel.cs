using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;

namespace LRReader.Shared.ViewModels
{
	public partial class SettingsPageViewModel : ObservableObject
	{
		private readonly ImagesService Images;
		private readonly PlatformService Platform;
		private readonly UpdatesService Updates;
		private readonly ApiService Api;
		private readonly TabsService Tabs;
		private readonly IKarenService Karen;

		public readonly string LRReader = "LRReader";

		public SettingsService SettingsManager { get; private set; }
		public Version Version => Platform.Version;
		public Version MinVersion => Updates.MIN_VERSION;
		public Version MaxVersion => Updates.MAX_VERSION;

		[ObservableProperty]
		private string? _shinobuStatusText;
		[ObservableProperty]
		private string? _shinobuPid;

		private CheckForUpdatesResult? checkResult;

		[ObservableProperty]
		private UpdateChangelog _changelog = default!;
		[ObservableProperty]
		private bool _showChangelog;
		[ObservableProperty]
		private double _updateProgress;
		[ObservableProperty]
		private ServerInfo? _serverInfo;
		[ObservableProperty]
		private string? _updateMessage;
		[ObservableProperty]
		private string? _updateError;

		private string? _contentFolder;
		public string? ContentFolder
		{
			get => _contentFolder;
			set
			{
				if (value != null && SetProperty(ref _contentFolder, value) && SettingsManager.Profile.Integration)
					SaveSetting(value);
			}
		}

		private string? _thumbnailFolder;
		public string? ThumbnailFolder
		{
			get => _thumbnailFolder;
			set
			{
				if (value != null && SetProperty(ref _thumbnailFolder, value) && SettingsManager.Profile.Integration)
					SaveSetting(value);
			}
		}

		private bool _startServerAutomatically;
		public bool StartServerAutomatically
		{
			get => _startServerAutomatically;
			set
			{
				if (SetProperty(ref _startServerAutomatically, value) && SettingsManager.Profile.Integration)
					SaveSetting(value);
			}
		}

		private bool _startWithWindows;
		public bool StartWithWindows
		{
			get => _startWithWindows;
			set
			{
				if (SetProperty(ref _startWithWindows, value) && SettingsManager.Profile.Integration)
					SaveSetting(value);
			}
		}

		public int _networkPort;
		public int NetworkPort
		{
			get => _networkPort;
			set
			{
				if (SetProperty(ref _networkPort, value) && SettingsManager.Profile.Integration)
					SaveSetting(value.ToString());
			}
		}

		public bool _forceDebugMode;
		public bool ForceDebugMode
		{
			get => _forceDebugMode;
			set
			{
				if (SetProperty(ref _forceDebugMode, value) && SettingsManager.Profile.Integration)
					SaveSetting(value);
			}
		}

		public bool _useWSL2;
		public bool UseWSL2
		{
			get => _useWSL2;
			set
			{
				if (SetProperty(ref _useWSL2, value) && SettingsManager.Profile.Integration)
					SaveSetting(value);
			}
		}
		[ObservableProperty]
		private bool _karenStatus;

		public ObservableCollection<string> SortBy = new ObservableCollection<string>();
		private int _sortByIndex = -1;
		public int SortByIndex
		{
			get => SortBy.IndexOf(SettingsManager.SortByDefault);
			set
			{
				if (value != _sortByIndex)
				{
					_sortByIndex = value;
					if (value == -1)
						SettingsManager.SortByDefault = "title";
					else
						SettingsManager.SortByDefault = SortBy.ElementAt(value);
					OnPropertyChanged("SortByIndex");
				}
			}
		}
		public string ThumbnailCacheSize = "";
		[ObservableProperty]
		private bool _progressCache;

		public MinionJob? thumbnailJob;

		public bool AvifMissing;
		public bool HeifMissing;

		public SettingsPageViewModel(SettingsService settings, ImagesService images, ArchivesService archives, PlatformService platform, UpdatesService updates, ApiService api, TabsService tabs, IKarenService karen)
		{
			SettingsManager = settings;
			Images = images;
			Platform = platform;
			Updates = updates;
			Api = api;
			Tabs = tabs;
			Karen = karen;

			foreach (var n in archives.Namespaces)
				SortBy.Add(n);
			_sortByIndex = SortBy.IndexOf(SettingsManager.SortByDefault);
		}

		public async Task CheckForPackages()
		{
			SetProperty(ref AvifMissing, !await Platform.CheckAppInstalled("Microsoft.AV1VideoExtension_8wekyb3d8bbwe"), nameof(AvifMissing));
			SetProperty(ref HeifMissing, !await Platform.CheckAppInstalled("Microsoft.HEIFImageExtension_8wekyb3d8bbwe"), nameof(HeifMissing));
		}


		public async Task<DownloadPayload?> DownloadDB()
		{
			return await DatabaseProvider.BackupJSON();
		}

		public async Task UpdateShinobuStatus()
		{
			var worker = Platform.GetLocalizedString("Settings/Server/WorkerStatus/Text");
			var status = Platform.GetLocalizedString("Settings/Server/WorkerUnknown/Text");
			var pid = "";
			if (SettingsManager.Profile.HasApiKey)
			{
				var result = await ShinobuProvider.GetShinobuStatus();
				if (result != null && result.pid != 0)
				{
					if (result.is_alive)
						status = Platform.GetLocalizedString("Settings/Server/WorkerRunning/Text");
					else
						status = Platform.GetLocalizedString("Settings/Server/WorkerStopped/Text");
					pid = $"PID: {result.pid}";
				}
			}
			ShinobuStatusText = worker + status;
			ShinobuPid = pid;
		}

		[RelayCommand]
		public async Task CheckForUpdates()
		{
			UpdateMessage = "";
			UpdateError = "";
			checkResult = await Updates.CheckForUpdates();
			if (checkResult.Result)
			{
				if (checkResult.Target != null)
					Changelog = await Updates.GetChangelog(checkResult.Target);
				else
					Changelog = new UpdateChangelog { Name = "", Content = Platform.GetLocalizedString("Settings/Updates/NoChangelog") };
				ShowChangelog = true;
			}
			else
			{
				UpdateMessage = checkResult.ErrorMessage!;
				UpdateError = Platform.GetLocalizedString("Pages/LoadingPage/UpdateErrorCode").AsFormat(checkResult.ErrorCode);
			}
		}

		[RelayCommand]
		public async Task InstallUpdate()
		{
			UpdateMessage = "";
			UpdateError = "";
			var result = await Updates.DownloadAndInstall(new Progress<double>(progress => UpdateProgress = progress), checkResult);
			if (!result.Result)
			{
				UpdateMessage = result.ErrorMessage!;
				UpdateError = Platform.GetLocalizedString("Pages/LoadingPage/UpdateErrorCode").AsFormat(result.ErrorCode);
			}
		}

		private async void SaveSetting(object value, [CallerMemberName] string? propertyName = null) => await Karen.SaveSetting((SettingType)Enum.Parse(typeof(SettingType), propertyName), value);

		public async Task UpdateServerInfo()
		{
			ServerInfo = await ServerProvider.GetServerInfo();
			if (SettingsManager.Profile.Integration)
			{
				ContentFolder = await Karen.LoadSetting<string>(SettingType.ContentFolder);
				ThumbnailFolder = await Karen.LoadSetting<string>(SettingType.ThumbnailFolder);
				StartServerAutomatically = await Karen.LoadSetting<bool>(SettingType.StartServerAutomatically);
				StartWithWindows = await Karen.LoadSetting<bool>(SettingType.StartWithWindows);
				NetworkPort = int.Parse(await Karen.LoadSetting<string>(SettingType.NetworkPort) ?? "0");
				ForceDebugMode = await Karen.LoadSetting<bool>(SettingType.ForceDebugMode);
				UseWSL2 = await Karen.LoadSetting<bool>(SettingType.UseWSL2);
				KarenStatus = Karen.IsConnected;
			}
		}

		public async Task CheckThumbnailJob()
		{
			if (thumbnailJob == null)
				return;
			var status = await ServerProvider.GetMinionStatus(thumbnailJob.job);
			if (status == null)
			{
				thumbnailJob = null;
				return;
			}
			if (status.state!.Equals("finished"))
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("Thumbnail generation completed", ""));
				thumbnailJob = null;
			}
		}

		public async Task UpdateThumbnailCacheSize()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			ThumbnailCacheSize = await Images.GetThumbnailCacheSize();
			OnPropertyChanged("ThumbnailCacheSize");
			ProgressCache = false;
		}

		[RelayCommand]
		private async Task AddProfile()
		{
			var dialog = Platform.CreateDialog<ICreateProfileDialog>(Dialog.ServerProfile, false);
			var result = await dialog.ShowAsync();
			if (result == IDialogResult.Primary)
			{
				var address = dialog.Address;
				if (!(address.StartsWith("http://") || address.StartsWith("https://")))
					address = "http://" + address;
				SettingsManager.AddProfile(dialog.Name, address, dialog.ApiKey, dialog.Integration);
			}
		}

		[RelayCommand]
		private async Task EditProfile(ServerProfile profile)
		{
			var dialog = Platform.CreateDialog<ICreateProfileDialog>(Dialog.ServerProfile, true);
			dialog.Name = profile.Name;
			dialog.Address = profile.ServerAddress;
			dialog.ApiKey = profile.ServerApiKey;
			dialog.Integration = profile.Integration;
			var result = await dialog.ShowAsync();
			if (result == IDialogResult.Primary)
			{
				var address = dialog.Address;
				if (!(address.StartsWith("http://") || address.StartsWith("https://")))
					address = "http://" + address;
				SettingsManager.ModifyProfile(profile.UID, dialog.Name, address, dialog.ApiKey, dialog.Integration);
				Api.RefreshSettings(profile);
			}
		}

		[RelayCommand]
		private async Task RemoveProfile(ServerProfile profile)
		{
			var shouldRestart = SettingsManager.Profile?.Equals(profile) ?? false;

			var result = await Platform.OpenGenericDialog(
				Platform.GetLocalizedString("Settings/Profiles/RemoveDialog/Title"),
				Platform.GetLocalizedString("Settings/Profiles/RemoveDialog/PrimaryButtonText"),
				closebutton: Platform.GetLocalizedString("Settings/Profiles/RemoveDialog/CloseButtonText"),
				content: Platform.GetLocalizedString("Settings/Profiles/RemoveDialog/Content").AsFormat(profile.Name));
			if (result == IDialogResult.Primary)
			{
				SettingsManager.Profiles.Remove(profile);
				if (SettingsManager.Profiles.Count == 0 || shouldRestart)
				{
					//Delete metadata folder
					Tabs.CloseAllTabs();
					Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
				}
			}
		}

		[RelayCommand]
		private void ContinueProfile(ServerProfile profile)
		{
			SettingsManager.Profile = profile;
			SettingsManager.FirstStartup = false;
			Platform.GoToPage(Pages.Loading, PagesTransition.DrillIn);
		}

		[RelayCommand]
		private async Task RestartWorker() => await ShinobuProvider.RestartWorker();

		[RelayCommand]
		private async Task StopWorker() => await ShinobuProvider.StopWorker();

		[RelayCommand]
		private async Task RescanContent() => await ShinobuProvider.Rescan();

		[RelayCommand]
		private async Task ClearAllNew() => await DatabaseProvider.ClearAllNew();

		[RelayCommand]
		private async Task ResetSearch() => await SearchProvider.DiscardCache();

		[RelayCommand]
		private async Task RegenThumbnails(bool force)
		{
			if (thumbnailJob == null)
				thumbnailJob = await ArchivesProvider.RegenerateThumbnails(force);
		}

		[RelayCommand]
		private async Task ClearThumbnailCache()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			await Images.DeleteThumbnailCache();
			ThumbnailCacheSize = await Images.GetThumbnailCacheSize();
			OnPropertyChanged("ThumbnailCacheSize");
			ProgressCache = false;
		}

		[RelayCommand]
		private void OpenWebTab(string path) => Tabs.OpenTab(Tab.Web, SettingsManager.Profile.ServerAddressBrowser + path);

		[RelayCommand]
		private Task OpenLink(string url) => Platform.OpenInBrowser(new Uri(url));
		[RelayCommand]
		private async Task Repair()
		{
			var res = await Karen.SendMessage(new Dictionary<string, object> { { "PacketType", (int)PacketType.InstanceRepair } });
			if (res != null)
			{
				Tabs.CloseAllTabs();
				Platform.GoToPage(Pages.FirstRun, PagesTransition.DrillIn);
			}
		}
	}
}
