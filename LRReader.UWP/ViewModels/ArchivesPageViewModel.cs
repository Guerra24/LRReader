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
			RefreshOnErrorButton = false;
			if (animate)
				LoadingArchives = true;
			ArchiveList.Clear();
			var result = await ArchivesProvider.LoadArchives();
			if (animate)
				LoadingArchives = false;
			if (result)
			{
				foreach (var b in SettingsManager.Profile.Bookmarks)
				{
					var archive = ArchiveList.FirstOrDefault(a => a.arcid == b.archiveID);
					if (archive != null)
						EventManager.CloseTabWithHeader(archive.title);
				}
				await Task.Run(async () =>
				{
					foreach (var a in ArchivesProvider.Archives)
						await DispatcherHelper.RunAsync(() => ArchiveList.Add(a));
				});
				foreach (var b in SettingsManager.Profile.Bookmarks)
				{
					var archive = ArchiveList.FirstOrDefault(a => a.arcid == b.archiveID);
					if (archive != null)
						EventManager.AddTab(new ArchiveTab(archive), false);
					else
						EventManager.ShowError("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "");
				}
			}
			else
				RefreshOnErrorButton = true;
			_internalLoadingArchives = false;
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
	}
}
