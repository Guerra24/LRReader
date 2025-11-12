using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public partial class ArchivesPageViewModel
	{
		private readonly SettingsService Settings;
		private readonly ArchivesService Archives;
		private readonly SessionService Session;

		public ArchivesPageViewModel(SettingsService settings, ArchivesService archives, SessionService session)
		{
			Settings = settings;
			Archives = archives;
			Session = session;
		}

		public async Task Refresh()
		{
			await Archives.ReloadArchives();
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

			switch (Settings.SessionMode)
			{
				case SessionMode.Never:
					break;
				case SessionMode.Ask:
					if (await Session.HasValidTabs())
						Session.ShowRestore = true;
					break;
				case SessionMode.Always:
					await Session.Restore();
					break;
			}

			if (Settings.OpenBookmarksStart)
				foreach (var b in Settings.Profile.Bookmarks)
				{
					var archive = await Archives.GetOrAddArchive(b.archiveID);
					if (archive != null)
						Archives.OpenTab(archive, false);
					else
						WeakReferenceMessenger.Default.Send(new ShowNotification($"Bookmarked archive with ID {b.archiveID} not found", null, severity: NotificationSeverity.Warning));
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
