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

		[ObservableProperty]
		private bool _loadingArchives = false;
		[ObservableProperty]
		private bool _refreshOnErrorButton = false;

		public ObservableCollection<Archive> ArchiveList = new ObservableCollection<Archive>();

		private bool _internalLoadingArchives;

		public bool Empty => ArchiveList.Count == 0;

		public BookmarksTabViewModel(ApiService api, SettingsService settings, ArchivesService archives, IDispatcherService dispatcher)
		{
			Api = api;
			Settings = settings;
			Archives = archives;
			Dispatcher = dispatcher;
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
						var archive = await Archives.GetOrAddArchive(b.archiveID);
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

		[RelayCommand]
		public async Task Migrate()
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
		}

		public void Receive(DeleteArchiveMessage message)
		{
			ArchiveList.Remove(message.Value);
		}

	}
}
