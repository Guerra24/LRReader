using GalaSoft.MvvmLight;
using LRReader.UWP.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class SettingsPageViewModel : ViewModelBase
	{
		public SettingsManager SettingsManager => SharedGlobal.SettingsManager;
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
					RaisePropertyChanged("SortByIndex");
				}
			}
		}
		public ControlFlags ControlFlags
		{
			get => SharedGlobal.ControlFlags;
		}

		public SettingsPageViewModel()
		{
			UpdateReleaseData();
			foreach (var n in SharedGlobal.ArchivesManager.Namespaces)
				SortBy.Add(n);
			_sortByIndex = SortBy.IndexOf(SettingsManager.SortByDefault);
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

		public async Task ResetSearch()
		{
			await SearchProvider.DiscardCache();
		}
	}
}
