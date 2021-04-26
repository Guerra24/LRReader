using LRReader.Shared.Internal;
using LRReader.Shared.Services;
using LRReader.UWP.Views.Tabs;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class ArchivesPageViewModel : SearchResultsViewModel
	{
		public ArchivesPageViewModel(SettingsService settings, EventsService events) : base(settings, events)
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
				var archive = SharedGlobal.ArchivesManager.GetArchive(b.archiveID);
				if (archive != null)
					Events.CloseTabWithId(archive.title);
			}
			await SharedGlobal.ArchivesManager.ReloadArchives();
			LoadBookmarks();
			Page = 0;
			LoadingArchives = false;
			_internalLoadingArchives = false;
		}

		public void LoadBookmarks()
		{
			SortBy.Clear();
			foreach (var n in SharedGlobal.ArchivesManager.Namespaces)
				SortBy.Add(n);
			SortByIndex = SortBy.IndexOf(Settings.SortByDefault);
			OrderBy = Settings.OrderByDefault;
			if (Settings.OpenBookmarksStart)
				if (SharedGlobal.ArchivesManager.Archives.Count > 0)
					foreach (var b in Settings.Profile.Bookmarks)
					{
						var archive = SharedGlobal.ArchivesManager.GetArchive(b.archiveID);
						if (archive != null)
							Events.AddTab(new ArchiveTab(archive), false);
						else
							Events.ShowNotification("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "");
					}
		}

	}
}
