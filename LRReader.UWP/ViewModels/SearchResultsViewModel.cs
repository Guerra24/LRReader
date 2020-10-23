﻿using GalaSoft.MvvmLight;
using static LRReader.Internal.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Internal;
using LRReader.UWP.Views.Tabs;
using GalaSoft.MvvmLight.Threading;
using LRReader.Shared.Providers;

namespace LRReader.UWP.ViewModels
{
	public class SearchResultsViewModel : ViewModelBase
	{
		private bool _loadingArchives = true;
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
		public bool HasNextPage => Page < TotalArchives / SharedGlobal.ServerInfo.archives_per_page && ControlsEnabled;
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
		private bool _untaggedOnly;
		public bool UntaggedOnly
		{
			get => _untaggedOnly;
			set
			{
				_untaggedOnly = value;
				RaisePropertyChanged("UntaggedOnly");
			}
		}
		public string Query = "";
		public Category Category = new Category() { id = "", search = "" };
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
		protected bool _internalLoadingArchives;
		public ObservableCollection<string> Suggestions = new ObservableCollection<string>();
		public ObservableCollection<string> SortBy = new ObservableCollection<string>();
		private int _sortByIndex = -1;
		public int SortByIndex
		{
			get => _sortByIndex;
			set
			{
				if (value != _sortByIndex)
				{
					_sortByIndex = value;
					RaisePropertyChanged("SortByIndex");
					RaisePropertyChanged("ShowClear");
				}
			}
		}
		public string Order = "asc";

		public SearchResultsViewModel()
		{
			foreach (var n in SharedGlobal.ArchivesManager.Namespaces)
				SortBy.Add(n);
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
			string sortby;
			if (SortByIndex == -1)
				sortby = "title";
			else
				sortby = SortBy.ElementAt(SortByIndex);
			var resultPage = await SearchProvider.Search(
				SharedGlobal.ServerInfo.archives_per_page, page, Query, string.IsNullOrEmpty(Category.search) ? Category.id : "", NewOnly, UntaggedOnly, sortby, Order);
			if (resultPage != null)
			{
				await Task.Run(async () =>
				{
					foreach (var a in resultPage.data)
					{
						var archive = SharedGlobal.ArchivesManager.Archives.FirstOrDefault(b => b.arcid == a.arcid);
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
