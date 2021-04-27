using LRReader.Shared.Services;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public class ArchivesPageViewModel : SearchResultsViewModel
	{
		public ArchivesPageViewModel(
			SettingsService settings,
			EventsService events,
			ArchivesService archives,
			IDispatcherService dispatcher,
			ApiService api) : base(settings, events, archives, dispatcher, api)
		{
		}

		public async Task Refresh()
		{
			if (_internalLoadingArchives)
				return;
			_internalLoadingArchives = true;
			RefreshOnErrorButton = false;
			ArchiveList.Clear();
			LoadingArchives = true;
			foreach (var b in Settings.Profile.Bookmarks)
			{
				var archive = Archives.GetArchive(b.archiveID);
				if (archive != null)
					Events.CloseTabWithId(archive.title);
			}
			await Archives.ReloadArchives();
			LoadBookmarks();
			Page = 0;
			LoadingArchives = false;
			_internalLoadingArchives = false;
		}

		public void LoadBookmarks()
		{
			SortBy.Clear();
			foreach (var n in Archives.Namespaces)
				SortBy.Add(n);
			SortByIndex = SortBy.IndexOf(Settings.SortByDefault);
			OrderBy = Settings.OrderByDefault;
			if (Settings.OpenBookmarksStart)
				if (Archives.Archives.Count > 0)
					foreach (var b in Settings.Profile.Bookmarks)
					{
						var archive = Archives.GetArchive(b.archiveID);
						//if (archive != null)
//							Events.AddTab(new ArchiveTab(archive), false);
	//					else
		//					Events.ShowNotification("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "");
					}
		}

	}
}
