using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Base
{
	public partial class ArchiveBaseViewModel : ObservableObject
	{
		protected readonly SettingsService Settings;
		protected readonly ArchivesService Archives;
		private readonly ApiService Api;
		private readonly PlatformService Platform;
		private readonly TabsService Tabs;

		[ObservableProperty]
		private bool _refreshOnErrorButton = false;

		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set
			{
				SetProperty(ref _controlsEnabled, value);
				OnPropertyChanged("Downloading");
			}
		}
		private Archive _archive = new Archive() { arcid = "", isnew = "" };
		public Archive Archive
		{
			get => _archive;
			set
			{
				if (SetProperty(ref _archive, value))
				{
					BookmarkedArchive = Settings.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(Archive.arcid));
					OnPropertyChanged("IsNew");
				}
			}
		}
		public bool IsNew
		{
			get => _archive != null ? _archive.IsNewArchive() : false;
		}
		protected bool _downloading;
		public virtual bool Downloading
		{
			get => _downloading;
			set => SetProperty(ref _downloading, value);
		}

		private BookmarkedArchive _bookmarkedArchive = new BookmarkedArchive() { totalPages = -1 };
		public BookmarkedArchive BookmarkedArchive
		{
			get => _bookmarkedArchive;
			set
			{
				if (value != null)
				{
					_bookmarkedArchive = value;
				}
				else
				{
					_bookmarkedArchive = new BookmarkedArchive() { totalPages = -1 };
				}
				OnPropertyChanged("Bookmarked");
				OnPropertyChanged("Pages");
				OnPropertyChanged("BookmarkedArchive");
			}
		}
		public bool Bookmarked
		{
			get
			{
				return BookmarkedArchive.Bookmarked;
			}
			set
			{
				if (value != BookmarkedArchive.Bookmarked)
				{
					if (value)
					{
						var exist = Settings.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(Archive.arcid));
						if (exist != null)
						{
							BookmarkedArchive = exist;
						}
						else
						{
							Settings.Profile.Bookmarks.Add(BookmarkedArchive = new BookmarkedArchive() { archiveID = Archive.arcid, totalPages = Pages });
						}
					}
					else
					{
						Settings.Profile.Bookmarks.RemoveAll(b => b.archiveID.Equals(Archive.arcid));
						BookmarkedArchive = new BookmarkedArchive() { totalPages = -1 };
					}
					Settings.SaveProfiles();
					OnPropertyChanged("Icon");
				}
			}
		}
		private int _pages;
		public int Pages
		{
			get
			{
				int pages = _pages;
				if (Bookmarked)
					pages = BookmarkedArchive.totalPages > 0 ? BookmarkedArchive.totalPages : _pages;
				if (Api.ControlFlags.V077 && pages == 0)
					pages = Archive.pagecount;
				_pages = pages;
				return _pages;
			}
			set
			{
				SetProperty(ref _pages, value);
			}
		}
		public int BookmarkProgress
		{
			get
			{
				if (Bookmarked)
					return BookmarkedArchive.page;
				return 0;
			}
			set
			{
				if (Bookmarked)
				{
					BookmarkedArchive.page = value;
					BookmarkedArchive.totalPages = Pages;
					BookmarkedArchive.Update();
					Settings.SaveProfiles();
				}
			}
		}
		public object Icon
		{
			get => Platform.GetSymbol(Bookmarked ? Symbols.Favorite : Symbols.Pictures);
		}

		public bool CanEdit => Settings.Profile.HasApiKey;

		public ArchiveBaseViewModel(SettingsService settings, ArchivesService archives, ApiService api, PlatformService platform, TabsService tabs)
		{
			Settings = settings;
			Archives = archives;
			Api = api;
			Platform = platform;
			Tabs = tabs;
		}

		public async Task LoadArchive()
		{
			var result = await ArchivesProvider.GetArchive(Archive.arcid);
			if (result != null)
			{
				Archive.title = result.title;
				Archive.tags = result.tags;
				Archive.pagecount = result.pagecount;
				Archive.progress = result.progress;
				Archive.UpdateTags();
				OnPropertyChanged("Archive");
			}
		}

		public async Task SetProgress(int progress)
		{
			var result = await ArchivesProvider.UpdateProgress(Archive.arcid, progress);
			if (result)
				Archive.progress = progress;
		}

		public async Task<DownloadPayload> DownloadArchive()
		{
			return await ArchivesProvider.DownloadArchive(Archive.arcid);
		}

		public async Task DeleteArchive()
		{
			await Archives.DeleteArchive(Archive.arcid);
		}

		[ICommand]
		public void EditArchive() => Tabs.OpenTab(Tab.ArchiveEdit, Archive);

		[ICommand]
		public async Task EditCategories() => await Platform.OpenDialog(Dialog.CategoryArchive, Archive.arcid, Archive.title);
	}
}
