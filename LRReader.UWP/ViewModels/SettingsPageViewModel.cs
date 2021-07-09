using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.UWP.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace LRReader.UWP.ViewModels
{
	public class SettingsPageViewModel : ObservableObject
	{
		private readonly ImagesService Images;
		private readonly EventsService Events;
		private readonly ApiService Api;
		private readonly IPlatformService Platform;

		private ResourceLoader lang = ResourceLoader.GetForCurrentView("Settings");

		public readonly string LRReader = "LRReader";

		public SettingsService SettingsManager;
		public Version Version => Platform.Version;
		public Version MinVersion => UpdatesManager.MIN_VERSION;
		public Version MaxVersion => UpdatesManager.MAX_VERSION;

		private string _shinobuStatusText;
		public string ShinobuStatusText
		{
			get => _shinobuStatusText;
			set => SetProperty(ref _shinobuStatusText, value);
		}

		private string _shinobuPid;
		public string ShinobuPid
		{
			get => _shinobuPid;
			set => SetProperty(ref _shinobuPid, value);
		}

		public ReleaseInfo ReleaseInfo;
		private bool _showReleaseInfo;
		public bool ShowReleaseInfo
		{
			get => _showReleaseInfo;
			set => SetProperty(ref _showReleaseInfo, value);
		}
		private ServerInfo _serverInfo;
		public ServerInfo ServerInfo
		{
			get => _serverInfo;
			set => SetProperty(ref _serverInfo, value);
		}
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
		public string ThumbnailCacheSize;
		private bool _progressCache;
		public bool ProgressCache
		{
			get => _progressCache;
			set => SetProperty(ref _progressCache, value);
		}
		public MinionJob thumbnailJob;

		public ControlFlags ControlFlags
		{
			get => Api.ControlFlags;
		}

		public bool AvifMissing;
		public bool HeifMissing;

		public SettingsPageViewModel(SettingsService settings, ImagesService images, EventsService events, ArchivesService archives, ApiService api, IPlatformService platform)
		{
			SettingsManager = settings;
			Images = images;
			Events = events;
			Api = api;
			Platform = platform;

			UpdateReleaseData();
			foreach (var n in archives.Namespaces)
				SortBy.Add(n);
			_sortByIndex = SortBy.IndexOf(SettingsManager.SortByDefault);
		}

		public async Task CheckForPackages()
		{
			SetProperty(ref AvifMissing, !await (Platform as UWPlatformService).CheckAppInstalled("Microsoft.AV1VideoExtension_8wekyb3d8bbwe"), nameof(AvifMissing));
			SetProperty(ref HeifMissing, !await (Platform as UWPlatformService).CheckAppInstalled("Microsoft.HEIFImageExtension_8wekyb3d8bbwe"), nameof(HeifMissing));
		}

		public async Task RestartWorker()
		{
			await ShinobuProvider.RestartWorker();
		}

		public async Task StopWorker()
		{
			await ShinobuProvider.StopWorker();
		}

		public async Task<DownloadPayload> DownloadDB()
		{
			return await DatabaseProvider.BackupJSON();
		}

		public async Task ClearAllNew()
		{
			await DatabaseProvider.ClearAllNew();
		}

		public async Task UpdateShinobuStatus()
		{
			var worker = lang.GetString("Server/WorkerStatus/Text");
			var status = lang.GetString("Server/WorkerUnknown/Text");
			var pid = "";
			if (SettingsManager.Profile.HasApiKey)
			{
				var result = await ShinobuProvider.GetShinobuStatus();
				if (result?.pid != 0)
				{
					if (result.is_alive)
						status = lang.GetString("Server/WorkerRunning/Text");
					else
						status = lang.GetString("Server/WorkerStopped/Text");
					pid = $"PID: {result.pid}";
				}
			}
			ShinobuStatusText = $"{worker}{status}";
			ShinobuPid = pid;
		}

		public async void UpdateReleaseData()
		{
			var info = await SharedGlobal.UpdatesManager.CheckUpdates(Platform.Version);
			if (info != null)
			{
				SetProperty(ref ReleaseInfo, info, nameof(ReleaseInfo));
				ShowReleaseInfo = true;
			}
			else
			{
				ShowReleaseInfo = false;
				SetProperty(ref ReleaseInfo, null, nameof(ReleaseInfo));
			}
		}

		public async Task UpdateServerInfo()
		{
			var info = await ServerProvider.GetServerInfo();
			if (info != null)
			{
				ServerInfo = info;
			}
		}

		public async Task ResetSearch()
		{
			await SearchProvider.DiscardCache();
		}

		public async Task RegenThumbnails(bool force)
		{
			if (thumbnailJob == null)
				thumbnailJob = await ArchivesProvider.RegenerateThumbnails(force);
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
			if (status.state.Equals("finished"))
			{
				Events.ShowNotification("Thumbnail generation completed", "");
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
		public async Task ClearThumbnailCache()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			await Images.DeleteThumbnailCache();
			ThumbnailCacheSize = await Images.GetThumbnailCacheSize();
			OnPropertyChanged("ThumbnailCacheSize");
			ProgressCache = false;
		}
	}
}
