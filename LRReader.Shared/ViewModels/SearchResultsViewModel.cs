using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public delegate bool CustomArchiveCheck(Archive archive);

	public class SearchResultsViewModel : ObservableObject
	{
		protected readonly SettingsService Settings;
		protected readonly EventsService Events;
		protected readonly ArchivesService Archives;
		private readonly IDispatcherService Dispatcher;
		private readonly ApiService Api;

		public CustomArchiveCheck CustomArchiveCheckEvent = (a) => true;

		private bool _loadingArchives = true;
		public bool LoadingArchives
		{
			get => _loadingArchives;
			set
			{
				SetProperty(ref _loadingArchives, value);
				OnPropertyChanged("ControlsEnabled");
			}
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				SetProperty(ref _refreshOnErrorButton, value);
				OnPropertyChanged("ControlsEnabled");
			}
		}
		public ObservableCollection<Archive> ArchiveList { get; } = new ObservableCollection<Archive>();
		private int _page = 0;
		public int Page
		{
			get => _page;
			set => SetProperty(ref _page, value);
		}
		private int _totalArchives;
		public int TotalArchives
		{
			get => _totalArchives;
			set
			{
				SetProperty(ref _totalArchives, value);
				OnPropertyChanged("TotalPages");
			}
		}

		public int TotalPages => (int)Math.Max(Math.Ceiling(TotalArchives / (double)Api.ServerInfo.archives_per_page), 1);
		public bool HasNextPage => Page < TotalPages && ControlsEnabled;
		public bool HasPrevPage => Page > 0 && ControlsEnabled;

		private bool _newOnly;
		public bool NewOnly
		{
			get => _newOnly;
			set => SetProperty(ref _newOnly, value);
		}
		private bool _untaggedOnly;
		public bool UntaggedOnly
		{
			get => _untaggedOnly;
			set => SetProperty(ref _untaggedOnly, value);
		}
		public string Query = "";
		public Category Category = new Category() { id = "", search = "" };
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set => SetProperty(ref _controlsEnabled, value);
		}
		protected bool _internalLoadingArchives;
		public ObservableCollection<string> Suggestions = new ObservableCollection<string>();
		public ObservableCollection<string> SortBy = new ObservableCollection<string>();
		private int _sortByIndex = -1;
		public int SortByIndex
		{
			get => _sortByIndex;
			set => SetProperty(ref _sortByIndex, value);
		}
		public Order OrderBy = Order.Ascending;

		public SearchResultsViewModel(SettingsService settings, EventsService events, ArchivesService archives, IDispatcherService dispatcher, ApiService api)
		{
			Settings = settings;
			Events = events;
			Dispatcher = dispatcher;
			Archives = archives;
			Api = api;

			foreach (var n in Archives.Namespaces)
				SortBy.Add(n);
			SortByIndex = _sortByIndex = SortBy.IndexOf(Settings.SortByDefault);
			OrderBy = Settings.OrderByDefault;
			Events.DeleteArchiveEvent += DeleteArchive;
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
				Api.ServerInfo.archives_per_page, page, Query, string.IsNullOrEmpty(Category.search) ? Category.id : "", NewOnly, UntaggedOnly, sortby, OrderBy);
			if (resultPage != null)
			{
				TotalArchives = resultPage.Data.recordsFiltered;
				await Task.Run(async () =>
				{
					try
					{
						foreach (var a in resultPage.Data.data)
						{
							if (!CustomArchiveCheckEvent(a))
								continue;
							var archive = Archives.GetArchive(a.arcid);
							if (archive != null)
								await Dispatcher.RunAsync(() => ArchiveList.Add(archive), -10);
						}
					}
					catch (Exception e)
					{
						// TODO: Some instances are returning null archives, dump data and send back
						try
						{
							var json = ApiExtentions.CompressData(resultPage.Json);
							var attachments = new ErrorAttachmentLog[]
							{
							ErrorAttachmentLog.AttachmentWithBinary(json, "resultPage.json.bz2", "application/x-bzip2")
							};
							Events.ShowNotification("API is returning bad data", "An error log has been recorded for analysis", 0);
							Crashes.TrackError(e, attachments: attachments);
						}
						catch (Exception) { } // Just in case
					}
				});
			}
			else
				RefreshOnErrorButton = true;
			LoadingArchives = false;
			_internalLoadingArchives = false;
			ControlsEnabled = true;
		}

		public void DeleteArchive(string id)
		{
			ArchiveList.Remove(Archives.GetArchive(id));
		}
	}
}
