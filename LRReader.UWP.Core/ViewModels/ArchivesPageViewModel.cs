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
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using LRReader.Views.Tabs;
using RestSharp;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using static LRReader.Shared.Providers.Providers;
using static LRReader.Internal.Global;
using Microsoft.Toolkit.Collections;
using System.Threading;
using Microsoft.Toolkit.Uwp;
using LRReader.Shared.Internal;

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
				RaisePropertyChanged("HasNextPage");
				RaisePropertyChanged("HasPrevPage");
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
				RaisePropertyChanged("HasNextPage");
				RaisePropertyChanged("HasPrevPage");
			}
		}
		public ObservableCollection<Archive> ArchiveList = new ObservableCollection<Archive>();
		private int _page = 0;
		public int Page
		{
			get => _page;
			set
			{
				if (value != _page)
				{
					_page = value;
					RaisePropertyChanged("Page");
					RaisePropertyChanged("DisplayPage");
					RaisePropertyChanged("HasNextPage");
					RaisePropertyChanged("HasPrevPage");
				}
			}
		}
		public int DisplayPage => Page + 1;
		private int _totalArchives;
		public int TotalArchives
		{
			get => _totalArchives;
			set
			{
				if (value != _totalArchives)
				{
					_totalArchives = value;
					RaisePropertyChanged("TotalArchives");
				}
			}
		}
		public bool HasNextPage => Page < TotalArchives / 100 && ControlsEnabled; // TODO: Page Size
		public bool HasPrevPage => Page > 0 && ControlsEnabled;
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
		public string Query = "";
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set
			{
				_controlsEnabled = value;
				RaisePropertyChanged("ControlsEnabled");
				RaisePropertyChanged("HasNextPage");
				RaisePropertyChanged("HasPrevPage");
			}
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
			ControlsEnabled = false;
			_internalLoadingArchives = true;
			RefreshOnErrorButton = false;
			ArchiveList.Clear();
			if (animate)
				LoadingArchives = true;
			foreach (var b in SharedGlobal.SettingsManager.Profile.Bookmarks)
			{
				var archive = ArchivesProvider.Archives.FirstOrDefault(a => a.arcid == b.archiveID);
				if (archive != null)
					EventManager.CloseTabWithHeader(archive.title);
			}
			var result = await ArchivesProvider.LoadArchives();
			if (SharedGlobal.SettingsManager.OpenBookmarksStart)
				if (result)
					foreach (var b in SharedGlobal.SettingsManager.Profile.Bookmarks)
					{
						var archive = ArchivesProvider.Archives.FirstOrDefault(a => a.arcid == b.archiveID);
						if (archive != null)
							EventManager.AddTab(new ArchiveTab(archive), false);
						else
							EventManager.ShowError("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "");
					}
			var resultPage = await ArchivesProvider.GetArchivesForPage(Page = 0, "", false);
			if (resultPage != null)
			{
				await Task.Run(async () =>
				{
					foreach (var a in resultPage.data)
						await DispatcherHelper.RunAsync(() => ArchiveList.Add(a));
				});
				TotalArchives = resultPage.recordsFiltered;
			}
			else
				RefreshOnErrorButton = true;
			if (animate)
				LoadingArchives = false;
			_internalLoadingArchives = false;
			ControlsEnabled = true;
		}

		public async Task LoadTagStats()
		{
			TagStats.Clear();
			var result = await ArchivesProvider.LoadTagStats();
			if (result)
			{
				await Task.Run(() =>
				{
					foreach (var t in ArchivesProvider.TagStats)
						TagStats.Add(t);
				});
			}
		}

		public async Task NextPage()
		{
			if (HasNextPage)
				await LoadPage(Page + 1);
		}

		public async Task PrevPage()
		{
			if (HasPrevPage)
				await LoadPage(Page - 1);
		}

		public async Task ReloadSearch()
		{
			await LoadPage(0);
		}

		public async Task LoadPage(int page)
		{
			if (_internalLoadingArchives)
				return;
			ControlsEnabled = false;
			_internalLoadingArchives = true;
			RefreshOnErrorButton = false;
			LoadingArchives = true;
			ArchiveList.Clear();
			Page = page;
			var resultPage = await ArchivesProvider.GetArchivesForPage(page, Query, NewOnly);
			if (resultPage != null)
			{
				await Task.Run(async () =>
				{
					foreach (var a in resultPage.data)
					{
						var archive = ArchivesProvider.Archives.FirstOrDefault(b => b.arcid == a.arcid);
						await DispatcherHelper.RunAsync(() => ArchiveList.Add(archive));
					}
				});
				TotalArchives = resultPage.recordsFiltered;
			}
			else
				RefreshOnErrorButton = true;
			LoadingArchives = false;
			_internalLoadingArchives = false;
			ControlsEnabled = true;
		}
	}
}
