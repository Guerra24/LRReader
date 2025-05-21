using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;

namespace LRReader.Shared.ViewModels
{
	public partial class BookmarksTabViewModel : ObservableObject, IRecipient<DeleteArchiveMessage>
	{
		private readonly ApiService Api;
		private readonly SettingsService Settings;
		private readonly ArchivesService Archives;
		private readonly IDispatcherService Dispatcher;
		private readonly PlatformService Platform;

		public ObservableCollection<Archive> ArchiveList = new ObservableCollection<Archive>();

		private bool _internalLoadingArchives;

		public bool Empty => ArchiveList.Count == 0;

		public BookmarksTabViewModel(ApiService api, SettingsService settings, ArchivesService archives, IDispatcherService dispatcher, PlatformService platform)
		{
			Api = api;
			Settings = settings;
			Archives = archives;
			Dispatcher = dispatcher;
			Platform = platform;
			WeakReferenceMessenger.Default.Register(this);
		}

		[RelayCommand]
		public void BookmarkClick(GridViewExtParameter archive) => Archives.OpenTab((Archive)archive.Item, archive.Ctrl);

		[RelayCommand]
		public async Task Reload(bool animate = true)
		{
			if (_internalLoadingArchives)
				return;
			_internalLoadingArchives = true;
			ArchiveList.Clear();
			await Task.Run(async () =>
			{
				foreach (var b in Settings.Profile.Bookmarks)
				{
					var archive = await Archives.GetOrAddArchive(b.archiveID);
					if (archive != null)
						await Dispatcher.RunAsync(() => ArchiveList.Add(archive));
				}
			});
			OnPropertyChanged("Empty");
			_internalLoadingArchives = false;
		}

		[RelayCommand]
		public async Task Migrate()
		{
			if (!string.IsNullOrEmpty(Archives.BookmarkLink))
			{
				Settings.Profile.SynchronizeBookmarks = true;
				foreach (var bookmark in Settings.Profile.Bookmarks)
				{
					await CategoriesProvider.AddArchiveToCategory(Archives.BookmarkLink, bookmark.archiveID);
					if (Api.ControlFlags.ProgressTracking)
					{
						var archive = Archives.GetArchive(bookmark.archiveID);
						await ArchivesProvider.UpdateProgress(bookmark.archiveID, archive!.progress = bookmark.page + 1);
					}
				}
				Settings.SaveProfiles();
				WeakReferenceMessenger.Default.Send(new ShowNotification(Platform.GetLocalizedString("Tabs/Bookmarks/MigrationCompleted"), null));
			}
			else
			{
				await Platform.OpenGenericDialog(
					Platform.GetLocalizedString("Tabs/Bookmarks/MigrateDialog/Title"),
					Platform.GetLocalizedString("Tabs/Bookmarks/MigrateDialog/PrimaryButtonText"),
					content: Platform.GetLocalizedString("Tabs/Bookmarks/MigrateDialog/Content"));
			}
		}

		public void Receive(DeleteArchiveMessage message)
		{
			ArchiveList.Remove(message.Value);
		}

	}
}
