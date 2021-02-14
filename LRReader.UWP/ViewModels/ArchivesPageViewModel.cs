using LRReader.Shared.Internal;
using LRReader.UWP.Views.Tabs;
using System.Threading.Tasks;
using static LRReader.Internal.Global;

namespace LRReader.UWP.ViewModels
{
	public class ArchivesPageViewModel : SearchResultsViewModel
	{
		public async Task Refresh()
		{
			if (_internalLoadingArchives)
				return;
			_internalLoadingArchives = true;
			RefreshOnErrorButton = false;
			ArchiveList.Clear();
			LoadingArchives = true;
			foreach (var b in SharedGlobal.SettingsManager.Profile.Bookmarks)
			{
				var archive = SharedGlobal.ArchivesManager.GetArchive(b.archiveID);
				if (archive != null)
					EventManager.CloseTabWithHeader(archive.title);
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
			SortByIndex = SortBy.IndexOf(SharedGlobal.SettingsManager.SortByDefault);
			OrderBy = SharedGlobal.SettingsManager.OrderByDefault;
			if (SharedGlobal.SettingsManager.OpenBookmarksStart)
				if (SharedGlobal.ArchivesManager.Archives.Count > 0)
					foreach (var b in SharedGlobal.SettingsManager.Profile.Bookmarks)
					{
						var archive = SharedGlobal.ArchivesManager.GetArchive(b.archiveID);
						if (archive != null)
							EventManager.AddTab(new ArchiveTab(archive), false);
						else
							EventManager.ShowError("Bookmarked Archive with ID[" + b.archiveID + "] not found.", "");
					}
		}

	}
}
