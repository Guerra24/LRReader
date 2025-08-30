using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Extensions;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Base
{
	public partial class ArchiveBaseViewModel : ObservableObject
	{
		protected readonly SettingsService Settings;
		protected readonly ArchivesService Archives;
		protected readonly PlatformService Platform;
		protected readonly ApiService Api;
		private readonly TabsService Tabs;

		[ObservableProperty]
		private bool _refreshOnErrorButton;

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
		private Archive _archive = new Archive() { arcid = "", isnew = false };
		public Archive Archive
		{
			get => _archive;
			set
			{
				if (SetProperty(ref _archive, value))
				{
					var bookmark = Settings.Profile.Bookmarks.FirstOrDefault(b => Archive.arcid.Equals(b.archiveID));
					if (bookmark != null)
					{
						BookmarkedArchive = bookmark;
					}
					OnPropertyChanged("IsNew");
					OnPropertyChanged("Pages");
					OnPropertyChanged("Rating");
				}
			}
		}
		public bool IsNew
		{
			get => _archive != null ? _archive.isnew : false;
		}
		protected bool _downloading;
		public virtual bool Downloading
		{
			get => _downloading;
			set => SetProperty(ref _downloading, value);
		}

		private BookmarkedArchive _bookmarkedArchive = new BookmarkedArchive("") { totalPages = -1 };
		public BookmarkedArchive BookmarkedArchive
		{
			get => _bookmarkedArchive;
			set
			{
				if (SetProperty(ref _bookmarkedArchive, value ?? new BookmarkedArchive("") { totalPages = -1 }))
				{
					OnPropertyChanged("Bookmarked");
					OnPropertyChanged("Pages");
				}
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
							if (!string.IsNullOrEmpty(Archives.BookmarkLink) && Api.ControlFlags.V0940Edit)
								_ = CategoriesProvider.AddArchiveToCategory(Archives.BookmarkLink, Archive.arcid);
							Settings.Profile.Bookmarks.Add(BookmarkedArchive = new BookmarkedArchive(Archive.arcid) { totalPages = Pages });
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(Archives.BookmarkLink) && Api.ControlFlags.V0940Edit)
							_ = CategoriesProvider.RemoveArchiveFromCategory(Archives.BookmarkLink, Archive.arcid);
						Settings.Profile.Bookmarks.RemoveAll(b => b.archiveID.Equals(Archive.arcid));
						BookmarkedArchive = new BookmarkedArchive("") { totalPages = -1 };
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
				int pages = Archive.pagecount;
				if (Bookmarked && pages == 0)
					pages = BookmarkedArchive.totalPages > 0 ? BookmarkedArchive.totalPages : _pages;
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
			get => Platform.GetSymbol(Bookmarked ? Symbol.Favorite : Symbol.Pictures);
		}

		public double Rating
		{
			get => Archive.Rating;
			set
			{
				if (double.IsNaN(value))
					return;
				if (Archive.Rating == value)
					return;
				Archive.SetRating((int)value);
				OnPropertyChanged(nameof(Rating));
				Task.Run(async () => await ArchivesProvider.UpdateArchive(Archive.arcid, tags: Archive.tags).ConfigureAwait(false));
			}
		}

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
				Archive.isnew = result.isnew;
				Archive.pagecount = result.pagecount;
				Archive.progress = result.progress;
				Archive.extension = result.extension;
				Archive.lastreadtime = result.lastreadtime;
				Archive.size = result.size;
				Archive.filename = result.filename;
				Archive.summary = result.summary;
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

		public async Task<DownloadPayload?> DownloadArchive()
		{
			return await ArchivesProvider.DownloadArchive(Archive.arcid);
		}

		[RelayCommand]
		private void Edit() => Tabs.OpenTab(Tab.ArchiveEdit, Archive);

		[RelayCommand]
		private async Task EditCategories() => await Platform.OpenDialog(Dialog.CategoryArchive, Archive.arcid, Archive.title);

		[RelayCommand]
		private async Task Delete()
		{
			var result = await Platform.OpenGenericDialog(
				Platform.GetLocalizedString("Dialogs/RemoveArchive/Title").AsFormat(Archive.title),
				Platform.GetLocalizedString("Dialogs/RemoveArchive/PrimaryButtonText"),
				closebutton: Platform.GetLocalizedString("Dialogs/RemoveArchive/CloseButtonText"),
				content: Platform.GetLocalizedString("Dialogs/RemoveArchive/Content")
				);
			if (result == IDialogResult.Primary)
				await Archives.DeleteArchive(Archive.arcid);
		}

		[RelayCommand]
		private async Task TagClick(ArchiveTagsGroupTag tag)
		{
			if (tag.Namespace.Equals("source", StringComparison.OrdinalIgnoreCase))
			{
				if (Uri.TryCreate(tag.Tag.StartsWith("https://") || tag.Tag.StartsWith("http://") ? tag.Tag : $"https://{tag.Tag}", UriKind.Absolute, out var result))
				{
					var dialogResult = await Platform.OpenGenericDialog(
						Platform.GetLocalizedString("Dialogs/OpenLink/Title"),
						Platform.GetLocalizedString("Dialogs/OpenLink/PrimaryButtonText"),
						Platform.GetLocalizedString("Dialogs/OpenLink/SecondaryButtonText"),
						Platform.GetLocalizedString("Dialogs/OpenLink/CloseButtonText"),
						result.AbsoluteUri
						);
					switch (dialogResult)
					{
						case IDialogResult.Primary:
							await Platform.OpenInBrowser(result);
							break;
						case IDialogResult.Secondary:
							Platform.CopyToClipboard(result.AbsoluteUri);
							break;
					}
				}
			}
			else
			{
				Tabs.OpenTab(Tab.SearchResults, $"\"{tag.FullTag}\"$");
			}
		}

		public async Task OpenTab(IList<Archive> group)
		{
			if (Archive.IsTank)
			{
				var tank = await TankoubonsProvider.GetTankoubon(Archive.arcid);
				Tabs.OpenTab(Tab.Tankoubon, false, tank!.result);
			}
			else
			{
				Archives.OpenTab(Archive, false, group);
			}
		}

	}
}
