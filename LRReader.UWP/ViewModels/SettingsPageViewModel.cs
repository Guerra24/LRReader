using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class SettingsPageViewModel : ViewModelBase
	{
		public SettingsManager SettingsManager => SharedGlobal.SettingsManager;
		public Version Version => Util.GetAppVersion();
		public Version MinVersion => UpdatesManager.MIN_VERSION;
		public Version MaxVersion => UpdatesManager.MAX_VERSION;
		private string _cacheSizeInMB;
		public string CacheSizeInMB
		{
			get => _cacheSizeInMB;
			set
			{
				_cacheSizeInMB = value;
				RaisePropertyChanged("CacheSizeInMB");
			}
		}
		private bool _progressCache;
		public bool ProgressCache
		{
			get => _progressCache;
			set
			{
				_progressCache = value;
				RaisePropertyChanged("ProgressCache");
			}
		}
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
			set
			{
				_showReleaseInfo = value;
				RaisePropertyChanged("ShowReleaseInfo");
			}
		}
		private ServerInfo _serverInfo;
		public ServerInfo ServerInfo
		{
			get => _serverInfo;
			set
			{
				_serverInfo = value;
				RaisePropertyChanged("ServerInfo");
			}
		}

		public SettingsPageViewModel()
		{
			UpdateReleaseData();
		}

		public async Task UpdateCacheSize()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			CacheSizeInMB = await Global.ImageManager.GetCacheSizeMB();
			ProgressCache = false;
		}

		public async Task ClearCache()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			await Global.ImageManager.ClearCache();
			ProgressCache = false;
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
					RaisePropertyChanged("ShinobuPID");
				}
				else
				{
					_shinobuStatus.pid = 0;
					_shinobuStatus.is_alive = 0;
				}
			}
			RaisePropertyChanged("ShinobuRunning");
			RaisePropertyChanged("ShinobuStopped");
			RaisePropertyChanged("ShinobuUnknown");
		}

		public async void UpdateReleaseData()
		{
			var info = await SharedGlobal.UpdatesManager.CheckUpdates(Util.GetAppVersion());
			if (info != null)
			{
				ReleaseInfo = info;
				RaisePropertyChanged("ReleaseInfo");
				ShowReleaseInfo = true;
			}
			else
			{
				ShowReleaseInfo = false;
				ReleaseInfo = null;
				RaisePropertyChanged("ReleaseInfo");
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
	}
}
