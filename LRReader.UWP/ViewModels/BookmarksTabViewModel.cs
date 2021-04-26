using LRReader.Shared.Models.Main;
using LRReader.UWP.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static LRReader.Shared.Internal.SharedGlobal;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.ViewModels
{
	public class BookmarksTabViewModel : ObservableObject
	{
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

		public BookmarksTabViewModel()
		{
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
			if (ArchivesManager.Archives.Count > 0)
			{
				await Task.Run(async () =>
				{
					foreach (var b in Settings.Profile.Bookmarks)
					{
						var archive = ArchivesManager.GetArchive(b.archiveID);
						if (archive != null)
							await DispatcherService.RunAsync(() => ArchiveList.Add(archive));
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
			ArchiveList.Remove(ArchivesManager.GetArchive(id));
		}

	}
}
