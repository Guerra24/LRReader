using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace LRReader.ViewModels.Base
{
	public class ArchiveBaseViewModel : ViewModelBase
	{
		private bool _isLoading = false;
		public bool IsLoading
		{
			get
			{
				return _isLoading;
			}
			set
			{
				_isLoading = value;
				RaisePropertyChanged("IsLoading");
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
		private bool _downloading;
		public bool Downloading
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
					RaisePropertyChanged("Bookmarked");
					RaisePropertyChanged("Pages");
					RaisePropertyChanged("BookmarkedArchive");
				}
			}
		}
		public bool Bookmarked
		{
			get
			{
				return BookmarkedArchive.totalPages > 0;
			}
			set
			{
				if (value)
					Global.SettingsManager.Profile.Bookmarks.Add(BookmarkedArchive = new BookmarkedArchive() { archiveID = Archive.arcid, totalPages = Pages });
				else
				{
					Global.SettingsManager.Profile.Bookmarks.RemoveAll(b => b.archiveID.Equals(Archive.arcid));
					BookmarkedArchive = new BookmarkedArchive() { totalPages = -1 };
				}
				Global.SettingsManager.SaveProfiles();
				RaisePropertyChanged("Icon");
			}
		}
		private int _pages;
		public int Pages
		{
			get
			{
				if (Bookmarked)
					return BookmarkedArchive.totalPages > 0 ? BookmarkedArchive.totalPages : _pages;
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

		public async Task<DownloadPayload> DownloadArchive()
		{
			return await Archive.DownloadArchive();
		}
	}
}
