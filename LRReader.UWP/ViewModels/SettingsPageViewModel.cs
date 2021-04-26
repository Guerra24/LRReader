using LRReader.UWP.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using LRReader.Shared.Services;

namespace LRReader.UWP.ViewModels
{
	public class SettingsPageViewModel : ObservableObject
	{
		public SettingsService SettingsManager => Service.Settings;
		public Version Version => Util.GetAppVersion();
		public Version MinVersion => UpdatesManager.MIN_VERSION;
		public Version MaxVersion => UpdatesManager.MAX_VERSION;
		private ShinobuStatus _shinobuStatus = new ShinobuStatus();
		public bool ShinobuRunning => _shinobuStatus.is_alive == 1 && !ShinobuUnknown;
		public bool ShinobuStopped => _shinobuStatus.is_alive == 0 && !ShinobuUnknown;
		public bool ShinobuUnknown => !SettingsManager.Profile.HasApiKey || _shinobuStatus.pid == 0;
		public int ShinobuPID => _shinobuStatus.pid;
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
			get => SharedGlobal.ControlFlags;
		}

		public bool AvifMissing;
		public bool HeifMissing;

		public SettingsPageViewModel()
		{
			UpdateReleaseData();
			foreach (var n in SharedGlobal.ArchivesManager.Namespaces)
				SortBy.Add(n);
			_sortByIndex = SortBy.IndexOf(SettingsManager.SortByDefault);
		}

		public async Task CheckForPackages()
		{
			SetProperty(ref AvifMissing, !await Util.CheckAppInstalled("Microsoft.AV1VideoExtension_8wekyb3d8bbwe"), nameof(AvifMissing));
			SetProperty(ref HeifMissing, !await Util.CheckAppInstalled("Microsoft.HEIFImageExtension_8wekyb3d8bbwe"), nameof(HeifMissing));
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
			if (SettingsManager.Profile.HasApiKey)
			{
				var result = await ShinobuProvider.GetShinobuStatus();
				if (result != null)
				{
					_shinobuStatus = result;
					OnPropertyChanged("ShinobuPID");
				}
				else
				{
					_shinobuStatus.pid = 0;
					_shinobuStatus.is_alive = 0;
				}
			}
			OnPropertyChanged("ShinobuRunning");
			OnPropertyChanged("ShinobuStopped");
			OnPropertyChanged("ShinobuUnknown");
		}

		public async void UpdateReleaseData()
		{
			var info = await SharedGlobal.UpdatesManager.CheckUpdates(Util.GetAppVersion());
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

			if (status.state.Equals("finished"))
			{
				Service.Events.ShowNotification("Thumbnail generation completed", "");
				thumbnailJob = null;
			}
		}

		public async Task UpdateThumbnailCacheSize()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			ThumbnailCacheSize = await Service.Images.GetThumbnailCacheSize();
			OnPropertyChanged("ThumbnailCacheSize");
			ProgressCache = false;
		}
		public async Task ClearThumbnailCache()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			await Service.Images.DeleteThumbnailCache();
			ThumbnailCacheSize = await Service.Images.GetThumbnailCacheSize();
			OnPropertyChanged("ThumbnailCacheSize");
			ProgressCache = false;
		}
	}
}
