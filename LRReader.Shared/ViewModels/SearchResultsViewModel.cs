using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;

namespace LRReader.Shared.ViewModels
{
	public delegate bool CustomArchiveCheck(Archive archive);

	public partial class SearchResultsViewModel : ObservableObject, IRecipient<DeleteArchiveMessage>
	{
		protected readonly SettingsService Settings;
		protected readonly ArchivesService Archives;
		private readonly IDispatcherService Dispatcher;
		private readonly ApiService Api;

		public CustomArchiveCheck CustomArchiveCheckEvent = (a) => true;

		[ObservableProperty]
		[NotifyPropertyChangedFor("ControlsEnabled")]
		private bool _loadingArchives = true;
		[ObservableProperty]
		[NotifyPropertyChangedFor("ControlsEnabled")]
		private bool _refreshOnErrorButton = false;
		public ObservableCollection<Archive> ArchiveList { get; } = new ObservableCollection<Archive>();
		[ObservableProperty]
		private int _page = 0;
		[ObservableProperty]
		[NotifyPropertyChangedFor("TotalPages")]
		private int _totalArchives;

		public int TotalPages => (int)Math.Max(Math.Ceiling(TotalArchives / (double)Api.ServerInfo.archives_per_page), 1);
		public bool HasNextPage => Page < TotalPages - 1 && ControlsEnabled;
		public bool HasPrevPage => Page > 0 && ControlsEnabled;

		[ObservableProperty]
		private bool _newOnly;
		[ObservableProperty]
		private bool _untaggedOnly;

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
		[ObservableProperty]
		private int _sortByIndex = -1;
		public Order OrderBy = Order.Ascending;

		public SearchResultsViewModel(SettingsService settings, ArchivesService archives, IDispatcherService dispatcher, ApiService api)
		{
			Settings = settings;
			Dispatcher = dispatcher;
			Archives = archives;
			Api = api;

			foreach (var n in Archives.Namespaces)
				SortBy.Add(n);
			SortByIndex = _sortByIndex = SortBy.IndexOf(Settings.SortByDefault);
			OrderBy = Settings.OrderByDefault;
			WeakReferenceMessenger.Default.Register(this);
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
				Api.ServerInfo.archives_per_page, page, Query, Category.id, NewOnly, UntaggedOnly, sortby, OrderBy);
			if (resultPage != null)
			{
				TotalArchives = resultPage.recordsFiltered;
				await Task.Run(async () =>
				{
					foreach (var a in resultPage.data)
					{
						if (!CustomArchiveCheckEvent(a))
							continue;
						var archive = Archives.GetArchive(a.arcid);
						if (archive != null)
							await Dispatcher.RunAsync(() => ArchiveList.Add(archive), -10);
					}
				});
			}
			else
				RefreshOnErrorButton = true;
			LoadingArchives = false;
			_internalLoadingArchives = false;
			ControlsEnabled = true;
		}

		public void OpenRandom()
		{
			var list = Archives.Archives;
			if (list.Count <= 1)
				return;
			var random = new Random();
			var item = list.ElementAt(random.Next(list.Count - 1));
			Archives.OpenTab(item.Value);
		}

		public void Receive(DeleteArchiveMessage message)
		{
			ArchiveList.Remove(message.Value);
		}
	}
}
