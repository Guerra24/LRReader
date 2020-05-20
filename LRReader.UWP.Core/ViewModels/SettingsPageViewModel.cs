using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Api;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static LRReader.Shared.Providers.Providers;

namespace LRReader.UWP.ViewModels
{
	public class SettingsPageViewModel : ViewModelBase
	{
		public SettingsManager SettingsManager
		{
			get => Global.SettingsManager;
		}
		public string Version
		{
			get => Util.GetAppVersion();
		}
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
			await ServerProvider.RestartWorker();
		}
		public async Task StopWorker()
		{
			await ServerProvider.StopWorker();
		}
		public async Task<DownloadPayload> DownloadDB()
		{
			return await ServerProvider.DownloadDB();
		}
		public async Task ClearAllNew()
		{
			await ServerProvider.ClearAllNew();
		}
		public async Task UpdateShinobuStatus()
		{
			if (SettingsManager.Profile.HasApiKey)
			{
				var result = await ServerProvider.GetShinobuStatus();
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

		public async void UpdateReleaseData()
		{
			var info = await SharedGlobal.UpdatesManager.CheckUpdates(new Version(Util.GetAppVersion()));
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
	}
}
