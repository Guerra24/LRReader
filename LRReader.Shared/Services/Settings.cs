using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public partial class SettingsService : ObservableObject, IService
	{
		private readonly ISettingsStorageService SettingsStorage;
		private readonly IFilesService Files;
		private readonly PlatformService Platform;

		[ObservableProperty]
		private ObservableCollection<ServerProfile> _profiles;

		private ServerProfile _profile;
		public ServerProfile Profile
		{
			get => _profile;
			set
			{
				if (value != null)
					SettingsStorage.StoreObjectLocal("ProfileUID", value.UID);
				SetProperty(ref _profile, value);
			}
		}
		public bool ProfilesAvailable
		{
			get => Profiles.Count > 0;
		}
		public bool AtLeastOneProfile
		{
			get => Profiles.Count > 1;
		}
		public int DefaultZoom
		{
			get => SettingsStorage.GetObjectLocal("DefaultZoom", 100);
			set
			{
				SettingsStorage.StoreObjectLocal("DefaultZoom", value);
				OnPropertyChanged("DefaultZoom");
			}
		}
		public bool ReadRTL
		{
			get => SettingsStorage.GetObjectLocal("ReadRTL", false);
			set
			{
				SettingsStorage.StoreObjectLocal("ReadRTL", value);
				Service.Events.RebuildReaderImagesSet();
			}
		}
		public bool TwoPages
		{
			get => SettingsStorage.GetObjectLocal("TwoPages", false);
			set
			{
				SettingsStorage.StoreObjectLocal("TwoPages", value);
				OnPropertyChanged("TwoPages");
				Service.Events.RebuildReaderImagesSet();
			}
		}
		public bool BookmarkReminder
		{
			get => SettingsStorage.GetObjectRoamed("BookmarkReminder", true);
			set
			{
				SettingsStorage.StoreObjectRoamed("BookmarkReminder", value);
				OnPropertyChanged("BookmarkReminder");
			}
		}
		public BookmarkReminderMode BookmarkReminderMode
		{
			get => (BookmarkReminderMode)SettingsStorage.GetObjectRoamed("BookmarkReminderMode", (int)BookmarkReminderMode.New);
			set => SettingsStorage.StoreObjectRoamed("BookmarkReminderMode", (int)value);
		}
		public bool RemoveBookmark
		{
			get => SettingsStorage.GetObjectRoamed("RemoveBookmark", true);
			set => SettingsStorage.StoreObjectRoamed("RemoveBookmark", value);
		}
		public bool OpenBookmarksTab
		{
			get => SettingsStorage.GetObjectRoamed("OpenBookmarksTab", false);
			set => SettingsStorage.StoreObjectRoamed("OpenBookmarksTab", value);
		}
		public bool OpenBookmarksStart
		{
			get => SettingsStorage.GetObjectRoamed("OpenBookmarksStart", false);
			set => SettingsStorage.StoreObjectRoamed("OpenBookmarksStart", value);
		}
		public bool OpenReader
		{
			get => SettingsStorage.GetObjectRoamed("OpenReader", false);
			set => SettingsStorage.StoreObjectRoamed("OpenReader", value);
		}
		public int KeyboardScroll
		{
			get => SettingsStorage.GetObjectLocal("KeyboardScroll", 200);
			set => SettingsStorage.StoreObjectLocal("KeyboardScroll", value);
		}
		public bool FitToWidth
		{
			get => SettingsStorage.GetObjectRoamed("FitToWidth", false);
			set
			{
				SettingsStorage.StoreObjectRoamed("FitToWidth", value);
				OnPropertyChanged("FitToWidth");
			}
		}
		public int FitScaleLimit
		{
			get => SettingsStorage.GetObjectLocal("FitScaleLimit", 100);
			set
			{
				SettingsStorage.StoreObjectLocal("FitScaleLimit", value);
				OnPropertyChanged("FitScaleLimit");
			}
		}
		public AppTheme Theme
		{
			get => (AppTheme)SettingsStorage.GetObjectLocal("Theme", (int)AppTheme.System);
			set
			{
				SettingsStorage.StoreObjectLocal("Theme", (int)value);
				Platform.ChangeTheme(value);
			}
		}
		public bool CompactBookmarks
		{
			get => SettingsStorage.GetObjectRoamed("CompactBookmarks", true);
			set => SettingsStorage.StoreObjectRoamed("CompactBookmarks", value);
		}
		public bool OpenCategoriesTab
		{
			get => SettingsStorage.GetObjectRoamed("OpenCategoriesTab", false);
			set => SettingsStorage.StoreObjectRoamed("OpenCategoriesTab", value);
		}
		public string SortByDefault
		{
			get => SettingsStorage.GetObjectRoamed("SortByDefault", "title");
			set => SettingsStorage.StoreObjectRoamed("SortByDefault", value);
		}
		public Order OrderByDefault
		{
			get => (Order)SettingsStorage.GetObjectRoamed("OrderByDefault", (int)Order.Ascending);
			set => SettingsStorage.StoreObjectRoamed("OrderByDefault", (int)value);
		}
		public bool ReaderImageSetBuilder
		{
			get => SettingsStorage.GetObjectLocal("ReaderImageSetBuilder", true);
			set
			{
				SettingsStorage.StoreObjectLocal("ReaderImageSetBuilder", value);
				Service.Events.RebuildReaderImagesSet();
			}
		}
		public bool UseVisualTags
		{
			get => SettingsStorage.GetObjectRoamed("UseVisualTags", true);
			set => SettingsStorage.StoreObjectRoamed("UseVisualTags", value);
		}
		public bool ScrollToChangePage
		{
			get => SettingsStorage.GetObjectRoamed("ScrollToChangePage", false);
			set => SettingsStorage.StoreObjectRoamed("ScrollToChangePage", value);
		}
		public bool UseReaderBackground
		{
			get => SettingsStorage.GetObjectLocal("UseReaderBackground", false);
			set
			{
				SettingsStorage.StoreObjectLocal("UseReaderBackground", value);
				OnPropertyChanged("UseReaderBackground");
			}
		}
		public string ReaderBackground
		{
			get => SettingsStorage.GetObjectLocal("ReaderBackground", "#FF000000");
			set => SettingsStorage.StoreObjectLocal("ReaderBackground", value);
		}
		public bool AutoUpdate
		{
			get => SettingsStorage.GetObjectLocal("AutoUpdate", true);
			set => SettingsStorage.StoreObjectLocal("AutoUpdate", value);
		}
		public bool OpenNextArchive
		{
			get => SettingsStorage.GetObjectRoamed("OpenNextArchive", true);
			set => SettingsStorage.StoreObjectRoamed("OpenNextArchive", value);
		}

		public static readonly int CurrentLocalVersion = 4;
		public int SettingsVersionLocal
		{
			get => SettingsStorage.GetObjectLocal("SettingsVersion", CurrentLocalVersion);
			set => SettingsStorage.StoreObjectLocal("SettingsVersion", value);
		}
		public static readonly int CurrentRoamedVersion = 2;
		public int SettingsVersionRoamed
		{
			get => SettingsStorage.GetObjectRoamed("SettingsVersion", CurrentRoamedVersion);
			set => SettingsStorage.StoreObjectRoamed("SettingsVersion", value);
		}

		private Subject<bool> save = new Subject<bool>();

		public SettingsService(ISettingsStorageService settingsStorage, IFilesService files, PlatformService platform)
		{
			SettingsStorage = settingsStorage;
			Files = files;
			Platform = platform;
			save.Throttle(TimeSpan.FromMilliseconds(500))
				.Subscribe(async (n) =>
				{
					try
					{
						await Files.StoreFileSafe(Files.Local + "\\Profiles.json", JsonConvert.SerializeObject(Profiles));
					}
					catch (Exception e)
					{
						Crashes.TrackError(e);
					}
				});
		}

		public async Task Init()
		{
			if (File.Exists(Files.Local + "/Profiles.json"))
				Profiles = JsonConvert.DeserializeObject<ObservableCollection<ServerProfile>>(await Files.GetFile(Files.Local + "/Profiles.json"));
			if (Profiles == null)
				Profiles = new ObservableCollection<ServerProfile>();

			UpgradeSettings();

			UpgradeProfiles();

			SaveProfiles();

			Profiles.CollectionChanged += ProfilesChanges;

			var profile = SettingsStorage.GetObjectLocal<string>("ProfileUID");
			if (profile != null)
			{
				Profile = Profiles.FirstOrDefault(p => p.UID.Equals(profile));
			}
		}

		private void UpgradeProfiles()
		{
			foreach (var p in Profiles)
			{
				switch (p.Version)
				{
					case 0:
						p.Version = 1;
						p.Bookmarks = new List<BookmarkedArchive>();
						break;
				}
			}
		}

		private void UpgradeSettings()
		{
			int localVersion = SettingsVersionLocal;
			int roamedVersion = SettingsVersionRoamed;
			while (true)
			{
				switch (localVersion)
				{
					case 0:
						KeyboardScroll = SettingsStorage.GetObjectLocal("SpacebarScroll", 200);
						SettingsStorage.DeleteObjectLocal("SpacebarScroll");
						break;
					case 1:
						SettingsStorage.DeleteObjectLocal("ArchivesPerPage");
						break;
					case 2:
						SettingsStorage.DeleteObjectLocal("SwitchTabArchive");
						break;
					case 3:
						SettingsStorage.DeleteObjectLocal("ImageCaching");
						break;
				}
				if (localVersion >= CurrentLocalVersion - 1)
					break;
				localVersion++;
			}
			SettingsVersionLocal = CurrentLocalVersion;
			while (true)
			{
				switch (roamedVersion)
				{
					case 0:
						break;
					case 1:
						var profiles = SettingsStorage.GetObjectRoamed<string>("Profiles");
						if (profiles != null)
						{
							Profiles = JsonConvert.DeserializeObject<ObservableCollection<ServerProfile>>(profiles);
						}
						SettingsStorage.DeleteObjectRoamed("Profiles");
						break;
				}
				if (roamedVersion >= CurrentRoamedVersion - 1)
					break;
				roamedVersion++;
			}
			SettingsVersionRoamed = CurrentRoamedVersion;
		}

		private void ProfilesChanges(object sender, NotifyCollectionChangedEventArgs e)
		{
			SaveProfiles();
			OnPropertyChanged("ProfilesAvailable");
			OnPropertyChanged("AtLeastOneProfile");
		}

		public ServerProfile AddProfile(string name, string address, string apikey)
		{
			ServerProfile profile = new ServerProfile();
			profile.Name = name;
			profile.ServerAddress = address;
			profile.ServerApiKey = apikey;
			Profiles.Add(profile);
			return profile;
		}

		public void ModifyProfile(string uid, string name, string address, string apikey)
		{
			var profile = Profiles.FirstOrDefault(p => p.UID.Equals(uid));
			profile.Name = name;
			profile.ServerAddress = address;
			profile.ServerApiKey = apikey;
			profile.Update();
			SaveProfiles();
		}

		public void SaveProfiles()
		{
			save.OnNext(true);
		}

	}
	public enum BookmarkReminderMode
	{
		All, New
	}
	public enum AppTheme
	{
		System, Dark, Light
	}
}
