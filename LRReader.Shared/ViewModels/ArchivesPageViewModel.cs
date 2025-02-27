using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public class ArchivesPageViewModel : SearchResultsViewModel
	{

		public ArchivesPageViewModel(
			SettingsService settings,
			ArchivesService archives,
			TabsService tabs,
			IDispatcherService dispatcher,
			ApiService api) : base(settings, archives, dispatcher, api, tabs) { }

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
			await LoadBookmarks();
			Page = 0;
			LoadingArchives = false;
			_internalLoadingArchives = false;
		}

		public async Task LoadBookmarks()
		{
			/*SortBy.Clear();
			foreach (var n in Archives.Namespaces)
				SortBy.Add(n);
			SortByIndex = SortBy.IndexOf(Settings.SortByDefault);
			OrderBy = Settings.OrderByDefault;
			SuggestedTags.Clear();
			foreach (var tag in Archives.TagStats.OrderByDescending(t => t.weight).Take(Settings.MaxSuggestedTags).ToList())
				SuggestedTags.Add(tag.GetNamespacedTag());*/
			if (Settings.OpenBookmarksStart)
				foreach (var b in Settings.Profile.Bookmarks)
				{
					var archive = await Archives.GetOrAddArchive(b.archiveID);
					if (archive != null)
						Archives.OpenTab(archive, false);
					else
						WeakReferenceMessenger.Default.Send(new ShowNotification("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "", severity: NotificationSeverity.Warning));
				}
			if (Settings.UseIncrementalCaching)
			{
				await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
				await Task.WhenAll(Settings.Profile.MarkedAsNonDuplicated.Select(hit => Task.WhenAll(Archives.GetOrAddArchive(hit.Left), Archives.GetOrAddArchive(hit.Right))));
			}
			Settings.Profile.MarkedAsNonDuplicated.RemoveAll(hit => !(Archives.HasArchive(hit.Left) && Archives.HasArchive(hit.Right)));
		}

	}
}
