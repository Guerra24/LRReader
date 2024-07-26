using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;

namespace LRReader.Shared.ViewModels
{
	public class ArchivesPageViewModel : SearchResultsViewModel
	{

		private readonly TabsService Tabs;

		public ArchivesPageViewModel(
			SettingsService settings,
			ArchivesService archives,
			TabsService tabs,
			IDispatcherService dispatcher,
			ApiService api) : base(settings, archives, dispatcher, api)
		{
			Tabs = tabs;
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
					Tabs.CloseTabWithId(archive.title);
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
			SuggestedTags.Clear();
			foreach (var tag in Archives.TagStats.OrderByDescending(t => t.weight).Take(Settings.MaxSuggestedTags).ToList())
				SuggestedTags.Add(tag.GetNamespacedTag());
			if (Settings.OpenBookmarksStart && Archives.Archives.Count > 0)
				foreach (var b in Settings.Profile.Bookmarks)
				{
					var archive = Archives.GetArchive(b.archiveID);
					if (archive != null)
						Archives.OpenTab(archive, false);
					else
						WeakReferenceMessenger.Default.Send(new ShowNotification("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "", severity: NotificationSeverity.Warning));
				}
			Settings.Profile.MarkedAsNonDuplicated.RemoveAll(hit => !(Archives.HasArchive(hit.Left) && Archives.HasArchive(hit.Right)));
		}

	}
}
