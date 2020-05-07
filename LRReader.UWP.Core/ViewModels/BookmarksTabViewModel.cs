using GalaSoft.MvvmLight;
using LRReader.Shared.Models.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRReader.Shared.Providers.Providers;
using static LRReader.Shared.Internal.SharedGlobal;
using LRReader.UWP.Views.Tabs;
using GalaSoft.MvvmLight.Threading;

namespace LRReader.UWP.ViewModels
{
	public class BookmarksTabViewModel : ViewModelBase
	{
		private bool _isLoading = false;
		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				RaisePropertyChanged("IsLoading");
			}
		}
		private bool _loadingArchives = false;
		public bool LoadingArchives
		{
			get => _loadingArchives;
			set
			{
				_loadingArchives = value;
				RaisePropertyChanged("LoadingArchives");
			}
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				_refreshOnErrorButton = value;
				RaisePropertyChanged("RefreshOnErrorButton");
			}
		}
		public ObservableCollection<Archive> ArchiveList = new ObservableCollection<Archive>();

		private bool _internalLoadingArchives;

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
				await Task.Run(async () =>
				{
					foreach (var b in SettingsManager.Profile.Bookmarks)
					{
						var archive = ArchivesManager.Archives.FirstOrDefault(a => a.arcid == b.archiveID);
						if (archive != null)
							await DispatcherHelper.RunAsync(() => ArchiveList.Add(archive));
					}
				});
			else
				RefreshOnErrorButton = true;
			if (animate)
				LoadingArchives = false;
			_internalLoadingArchives = false;
		}

	}
}
