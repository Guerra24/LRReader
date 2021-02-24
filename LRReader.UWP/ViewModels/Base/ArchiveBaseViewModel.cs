using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace LRReader.UWP.ViewModels.Base
{
	public class ArchiveBaseViewModel : ViewModelBase
	{
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
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set
			{
				_controlsEnabled = value;
				RaisePropertyChanged("ControlsEnabled");
				RaisePropertyChanged("Downloading");
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
						BookmarkedArchive = Global.SettingsManager.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(Archive.arcid));
						RaisePropertyChanged("Archive");
						RaisePropertyChanged("IsNew");
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
				RaisePropertyChanged("Downloading");
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
				RaisePropertyChanged("Bookmarked");
				RaisePropertyChanged("Pages");
				RaisePropertyChanged("BookmarkedArchive");
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
						var exist = Global.SettingsManager.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(Archive.arcid));
						if (exist != null)
						{
							BookmarkedArchive = exist;
						}
						else
						{
							Global.SettingsManager.Profile.Bookmarks.Add(BookmarkedArchive = new BookmarkedArchive() { archiveID = Archive.arcid, totalPages = Pages });
						}
					}
					else
					{
						Global.SettingsManager.Profile.Bookmarks.RemoveAll(b => b.archiveID.Equals(Archive.arcid));
						BookmarkedArchive = new BookmarkedArchive() { totalPages = -1 };
					}
					Global.SettingsManager.SaveProfiles();
					RaisePropertyChanged("Icon");
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
					RaisePropertyChanged("Pages");
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
					Global.SettingsManager.SaveProfiles();
				}
			}
		}
		public SymbolIconSource Icon
		{
			get => new SymbolIconSource() { Symbol = Bookmarked ? Symbol.Favorite : Symbol.Pictures };
		}

		public bool CanEdit => Global.SettingsManager.Profile.HasApiKey;

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
				RaisePropertyChanged("Archive");
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
