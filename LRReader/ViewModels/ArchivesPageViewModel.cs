using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.Models.Api;
using LRReader.Models.Main;
using LRReader.Views.Tabs;
using RestSharp;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace LRReader.ViewModels
{
	public class ArchivesPageViewModel : ViewModelBase
	{
		private bool _isLoading = false;
		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				RaisePropertyChanged("IsLoading");
			}
		}
		private bool _loadingArchives = false;
		public bool LoadingArchives
		{
			get => _loadingArchives;
			set
			{
				_loadingArchives = value;
				RaisePropertyChanged("LoadingArchives");
				RaisePropertyChanged("ControlsEnabled");
			}
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				_refreshOnErrorButton = value;
				RaisePropertyChanged("RefreshOnErrorButton");
				RaisePropertyChanged("ControlsEnabled");
			}
		}
		public ObservableCollection<Archive> ArchiveList = new ObservableCollection<Archive>();
		private bool _newOnly;
		public bool NewOnly
		{
			get => _newOnly;
			set
			{
				_newOnly = value;
				RaisePropertyChanged("NewOnly");
			}
		}
		public bool ControlsEnabled
		{
			get => !LoadingArchives && !RefreshOnErrorButton;
		}
		private bool _internalLoadingArchives;
		public ObservableCollection<string> Suggestions = new ObservableCollection<string>();
		public ObservableCollection<TagStats> TagStats = new ObservableCollection<TagStats>();

		public async Task Refresh()
		{
			await Refresh(true);
		}

		public async Task Refresh(bool animate)
		{
			if (_internalLoadingArchives)
				return;
			_internalLoadingArchives = true;
			ArchiveList.Clear();
			if (animate)
				LoadingArchives = true;
			RefreshOnErrorButton = false;

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/archivelist");

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<List<Archive>>(r);

			if (animate)
				LoadingArchives = false;
			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				RefreshOnErrorButton = true;
				_internalLoadingArchives = false;
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					foreach (var b in Global.SettingsManager.Profile.Bookmarks)
					{
						var archive = ArchiveList.FirstOrDefault(a => a.arcid == b.archiveID);
						if (archive != null)
							Global.EventManager.CloseTabWithHeader(archive.title);
					}
					await Task.Run(async () =>
					{
						foreach (var a in result.Data.OrderBy(a => a.title))
							await DispatcherHelper.RunAsync(() => ArchiveList.Add(a));

					});
					foreach (var b in Global.SettingsManager.Profile.Bookmarks)
					{
						var archive = ArchiveList.FirstOrDefault(a => a.arcid == b.archiveID);
						if (archive != null)
							Global.EventManager.AddTab(new ArchiveTab(archive), false);
					}
					break;
				case HttpStatusCode.Unauthorized:
					RefreshOnErrorButton = true;
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}
			_internalLoadingArchives = false;
		}

		public async Task LoadTagStats()
		{
			TagStats.Clear();
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/tagstats");

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<List<TagStats>>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					await Task.Run(() =>
					{
						foreach (var a in result.Data.OrderByDescending(a => a.weight))
						{
							TagStats.Add(a);
						}
					});
					break;
				case HttpStatusCode.Unauthorized:
					RefreshOnErrorButton = true;
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}
		}
	}
}
