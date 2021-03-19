using LRReader.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.ViewModels.Base
{
	public class ArchiveBaseViewModel : ObservableObject
	{
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				_refreshOnErrorButton = value;
				OnPropertyChanged("RefreshOnErrorButton");
			}
		}
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set
			{
				_controlsEnabled = value;
				OnPropertyChanged("ControlsEnabled");
				OnPropertyChanged("Downloading");
			}
		}
		private Archive _archive = new Archive() { arcid = "", isnew = "" };
		public Archive Archive
		{
			get => _archive;
			set
			{
				if (_archive != value)
				{
					if (!_archive.arcid.Equals(value.arcid))
					{
						_archive = value;
						BookmarkedArchive = Settings.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(Archive.arcid));
						OnPropertyChanged("Archive");
						OnPropertyChanged("IsNew");
					}
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
			set
			{
				_downloading = value;
				OnPropertyChanged("Downloading");
			}
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
				if (Global.ControlFlags.V077 && pages == 0)
					pages = Archive.pagecount;
				_pages = pages;
				return _pages;
			}
			set
			{
				if (value != _pages)
				{
					_pages = value;
					OnPropertyChanged("Pages");
				}
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
		public SymbolIconSource Icon
		{
			get => new SymbolIconSource() { Symbol = Bookmarked ? Symbol.Favorite : Symbol.Pictures };
		}

		public bool CanEdit => Settings.Profile.HasApiKey;

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
	}
}
