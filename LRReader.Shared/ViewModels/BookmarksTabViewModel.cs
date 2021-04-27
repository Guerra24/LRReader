using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public class BookmarksTabViewModel : ObservableObject
	{
		private readonly EventsService Events;
		private readonly SettingsService Settings;
		private readonly ArchivesService Archives;
		private readonly IDispatcherService Dispatcher;

		private bool _loadingArchives = false;
		public bool LoadingArchives
		{
			get => _loadingArchives;
			set => SetProperty(ref _loadingArchives, value);
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set => SetProperty(ref _refreshOnErrorButton, value);
		}
		public ObservableCollection<Archive> ArchiveList = new ObservableCollection<Archive>();

		private bool _internalLoadingArchives;

		public bool Empty => ArchiveList.Count == 0;

		public BookmarksTabViewModel(EventsService events, SettingsService settings, ArchivesService archives, IDispatcherService dispatcher)
		{
			Events = events;
			Settings = settings;
			Archives = archives;
			Dispatcher = dispatcher;
			Events.DeleteArchiveEvent += DeleteArchive;
		}

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
			ArchiveList.Clear();
			if (animate)
				LoadingArchives = true;
			if (Archives.Archives.Count > 0)
			{
				await Task.Run(async () =>
				{
					foreach (var b in Settings.Profile.Bookmarks)
					{
						var archive = Archives.GetArchive(b.archiveID);
						if (archive != null)
							await Dispatcher.RunAsync(() => ArchiveList.Add(archive));
					}
				});
				OnPropertyChanged("Empty");
			}
			else
				RefreshOnErrorButton = true;
			if (animate)
				LoadingArchives = false;
			_internalLoadingArchives = false;
		}

		public void DeleteArchive(string id)
		{
			ArchiveList.Remove(Archives.GetArchive(id));
		}

	}
}
