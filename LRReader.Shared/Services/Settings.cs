using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Sentry;
#if WINDOWS_UWP
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.AccessCache;
#endif

namespace LRReader.Shared.Services
{
	public partial class SettingsService : ObservableObject, IService, IDisposable
	{
		private readonly ISettingsStorageService SettingsStorage;
		private readonly IFilesService Files;
		private readonly PlatformService Platform;

		[ObservableProperty]
		private ObservableCollection<ServerProfile> _profiles = new ObservableCollection<ServerProfile>();

		private ServerProfile _profile = null!;
		public ServerProfile Profile
		{
			get => _profile!;
			set
			{
				if (value != null)
					SettingsStorage.StoreObjectLocal(value.UID, "ProfileUID");
				SetProperty(ref _profile!, value);
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
			get => SettingsStorage.GetObjectLocal(100);
			set
			{
				SettingsStorage.StoreObjectLocal(value);
				OnPropertyChanged();
			}
		}
		public bool ReadRTL
		{
			get => SettingsStorage.GetObjectLocal(false);
			set
			{
				SettingsStorage.StoreObjectLocal(value);
				Service.Events.RebuildReaderImagesSet();
			}
		}
		public bool TwoPages
		{
			get => SettingsStorage.GetObjectLocal(false);
			set
			{
				SettingsStorage.StoreObjectLocal(value);
				OnPropertyChanged();
				Service.Events.RebuildReaderImagesSet();
			}
		}
		public bool BookmarkReminder
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set
			{
				SettingsStorage.StoreObjectRoamed(value);
				OnPropertyChanged();
			}
		}
		public BookmarkReminderMode BookmarkReminderMode
		{
			get => (BookmarkReminderMode)SettingsStorage.GetObjectRoamed((int)BookmarkReminderMode.New);
			set => SettingsStorage.StoreObjectRoamed((int)value);
		}
		public bool RemoveBookmark
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool OpenBookmarksTab
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool OpenBookmarksStart
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool OpenReader
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public int KeyboardScroll
		{
			get => SettingsStorage.GetObjectLocal(200);
			set => SettingsStorage.StoreObjectLocal(value);
		}
		public bool FitToWidth
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set
			{
				SettingsStorage.StoreObjectRoamed(value);
				OnPropertyChanged();
			}
		}
		public int FitScaleLimit
		{
			get => SettingsStorage.GetObjectLocal(100);
			set
			{
				SettingsStorage.StoreObjectLocal(value);
				OnPropertyChanged();
			}
		}
		public AppTheme Theme
		{
			get => (AppTheme)SettingsStorage.GetObjectLocal((int)AppTheme.System);
			set
			{
				SettingsStorage.StoreObjectLocal((int)value);
				Platform.ChangeTheme(value);
			}
		}
		public bool CompactBookmarks
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool OpenCategoriesTab
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public string SortByDefault
		{
			get => SettingsStorage.GetObjectRoamed("title");
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public Order OrderByDefault
		{
			get => (Order)SettingsStorage.GetObjectRoamed((int)Order.Ascending);
			set => SettingsStorage.StoreObjectRoamed((int)value);
		}
		public bool ReaderImageSetBuilder
		{
			get => SettingsStorage.GetObjectLocal(true);
			set
			{
				SettingsStorage.StoreObjectLocal(value);
				Service.Events.RebuildReaderImagesSet();
			}
		}
		public bool UseVisualTags
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool ScrollToChangePage
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool UseReaderBackground
		{
			get => SettingsStorage.GetObjectLocal(false);
			set
			{
				SettingsStorage.StoreObjectLocal(value);
				OnPropertyChanged();
			}
		}
		public string ReaderBackground
		{
			get => SettingsStorage.GetObjectLocal("#FF000000");
			set => SettingsStorage.StoreObjectLocal(value);
		}
		public bool AutoUpdate
		{
			get => SettingsStorage.GetObjectLocal(false);
			set => SettingsStorage.StoreObjectLocal(value);
		}
		public bool OpenNextArchive
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool AutoLogin
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool CrashReporting
		{
			get => SettingsStorage.GetObjectLocal(true);
			set => SettingsStorage.StoreObjectLocal(value);
		}
		public TagsPopupLocation TagsPopup
		{
			get => (TagsPopupLocation)SettingsStorage.GetObjectRoamed((int)TagsPopupLocation.Middle);
			set => SettingsStorage.StoreObjectRoamed((int)value);
		}
		public bool KeepPageDetailsOpen
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool ShowExtraPageDetails
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public bool UseVerticalReader
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set
			{
				SettingsStorage.StoreObjectRoamed(value);
				OnPropertyChanged();
			}
		}
		public bool UseVerticalTabs
		{
			get => SettingsStorage.GetObjectLocal(false);
			set => SettingsStorage.StoreObjectLocal(value);
		}
		public bool Autoplay
		{
			get => SettingsStorage.GetObjectLocal(false);
			set => SettingsStorage.StoreObjectLocal(value);
		}
		public int AutoplayStartDelay
		{
			get => SettingsStorage.GetObjectRoamed(2000);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public int AutoplayBeforeChangeDelay
		{
			get => SettingsStorage.GetObjectRoamed(2000);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public int AutoplayAfterChangeDelay
		{
			get => SettingsStorage.GetObjectRoamed(2000);
			set => SettingsStorage.StoreObjectRoamed(value);
		}
		public int AutoplaySpeed
		{
			get => SettingsStorage.GetObjectLocal(100);
			set
			{
				SettingsStorage.StoreObjectLocal(value);
				OnPropertyChanged();
			}
		}
		public string ProfilesPathLocation
		{
			get => SettingsStorage.GetObjectLocal(Path.Combine(Files.Local, "Profiles.json"));
			private set => SettingsStorage.StoreObjectLocal(value);
		}
#if WINDOWS_UWP
		private string ProfilesFileToken
		{
			get => SettingsStorage.GetObjectLocal("");
			set => SettingsStorage.StoreObjectLocal(value);
		}

		public StorageFile ProfilesFile { get; private set; } = null!;
#endif
		public bool ShowMap
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set => SettingsStorage.StoreObjectRoamed(value);
		}

		public int MaxSuggestedTags
		{
			get => SettingsStorage.GetObjectRoamed(25);
			set
			{
				SettingsStorage.StoreObjectRoamed(value);
				OnPropertyChanged();
			}
		}

		public bool ShowSuggestedTags
		{
			get => SettingsStorage.GetObjectRoamed(false);
			set
			{
				SettingsStorage.StoreObjectRoamed(value);
				OnPropertyChanged();
			}
		}

		public bool ReaderAnimations
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set
			{
				SettingsStorage.StoreObjectRoamed(value);
				OnPropertyChanged();
			}
		}

		public bool PageChangeAnimation
		{
			get => SettingsStorage.GetObjectRoamed(true);
			set
			{
				SettingsStorage.StoreObjectRoamed(value);
				OnPropertyChanged();
			}
		}

		public static readonly int CurrentLocalVersion = 4;
		public int SettingsVersionLocal
		{
			get => SettingsStorage.GetObjectLocal(CurrentLocalVersion);
			set => SettingsStorage.StoreObjectLocal(value);
		}
		public static readonly int CurrentRoamedVersion = 2;
		public int SettingsVersionRoamed
		{
			get => SettingsStorage.GetObjectRoamed(CurrentRoamedVersion);
			set => SettingsStorage.StoreObjectRoamed(value);
		}

		private Throttle<object> save;

		public bool FirstStartup = true;

		public SettingsService(ISettingsStorageService settingsStorage, IFilesService files, PlatformService platform)
		{
			SettingsStorage = settingsStorage;
			Files = files;
			Platform = platform;
			save = NotRx.CreateThrottledEvent(() =>
			{
				try
				{
#if WINDOWS_UWP
					FileIO.WriteTextAsync(ProfilesFile, JsonSerializer.Serialize(Profiles, JsonSettings.Options)).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
#else
					Files.StoreFileSafe(ProfilesPathLocation, JsonSerializer.Serialize(Profiles, JsonSettings.Options)).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
				}
				catch (Exception e)
				{
					SentrySdk.CaptureException(e);
				}
			});
		}

		public async Task Init()
		{
#if WINDOWS_UWP
			try
			{
				if (string.IsNullOrEmpty(ProfilesFileToken))
				{
					ProfilesFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Profiles.json", CreationCollisionOption.OpenIfExists);
				}
				else
					ProfilesFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(ProfilesFileToken);
			}
			catch
			{
				// Reset everything
				StorageApplicationPermissions.FutureAccessList.Remove(ProfilesFileToken);

				SettingsStorage.DeleteObjectLocal(nameof(ProfilesPathLocation));
				SettingsStorage.DeleteObjectLocal(nameof(ProfilesFileToken));

				ProfilesFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Profiles.json", CreationCollisionOption.ReplaceExisting);
			}

			ProfilesPathLocation = ProfilesFile.Path;
			var content = await FileIO.ReadTextAsync(ProfilesFile);
			Profiles = !string.IsNullOrEmpty(content) ? JsonSerializer.Deserialize<ObservableCollection<ServerProfile>>(content, JsonSettings.Options)! : [];
#else
			var content = await Files.GetFile(ProfilesPathLocation);
			if (File.Exists(ProfilesPathLocation))
				Profiles = !string.IsNullOrEmpty(content) ? JsonSerializer.Deserialize<ObservableCollection<ServerProfile>>(content, JsonSettings.Options)! : [];
#endif

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
					case 1:
						p.Version = 2;
						p.MarkedAsNonDuplicated = new List<ArchiveHit>();
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
						KeyboardScroll = SettingsStorage.GetObjectLocal(200, "SpacebarScroll");
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
							Profiles = JsonSerializer.Deserialize<ObservableCollection<ServerProfile>>(profiles, JsonSettings.Options) ?? new ObservableCollection<ServerProfile>();
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
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (var item in e.OldItems)
				{
					if (item is ServerProfile profile)
					{
						var path = $"{Files.LocalCache}/Metadata/{profile.UID}/";
						if (Directory.Exists(path))
							Directory.Delete(path, true);
					}
				}
			}
			OnPropertyChanged(nameof(ProfilesAvailable));
			OnPropertyChanged(nameof(AtLeastOneProfile));
		}

		public ServerProfile AddProfile(string name, string address, string apikey, bool integration)
		{
			var profile = new ServerProfile(name, address, apikey, integration);
			Profiles.Add(profile);
			return profile;
		}

		public void ModifyProfile(string uid, string name, string address, string apikey, bool integration)
		{
			var profile = Profiles.FirstOrDefault(p => p.UID.Equals(uid));
			profile.Name = name;
			profile.ServerAddress = address;
			profile.ServerApiKey = apikey;
			profile.Integration = integration;
			profile.Update();
			SaveProfiles();
		}

		public void SaveProfiles()
		{
			save.Action(null!);
		}

		public void Dispose()
		{
			save.Lock.Dispose();
		}

		public async Task ChangeProfilesLocation()
		{
#if WINDOWS_UWP
			var savePicker = new FileSavePicker();
			savePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			savePicker.FileTypeChoices.Add("JSON file", new List<string> { ".json" });
			savePicker.SuggestedFileName = "Profiles.json";
			var file = await savePicker.PickSaveFileAsync();
			if (file != null)
			{
				if (file.IsEqual(ProfilesFile))
					return;

				await ProfilesFile.MoveAndReplaceAsync(file);
				StorageApplicationPermissions.FutureAccessList.Remove(ProfilesFileToken);
				ProfilesFileToken = StorageApplicationPermissions.FutureAccessList.Add(file);
				ProfilesFile = file;
				ProfilesPathLocation = file.Path;
			}
#else
#endif
			OnPropertyChanged(nameof(ProfilesPathLocation));
		}

		public async Task ResetProfilesLocation()
		{
			if (Path.Combine(Files.Local, "Profiles.json").Equals(ProfilesPathLocation))
				return;
#if WINDOWS_UWP
			var original = await ApplicationData.Current.LocalFolder.CreateFileAsync("Profiles.json", CreationCollisionOption.ReplaceExisting);
			await ProfilesFile.MoveAndReplaceAsync(original);
			ProfilesFile = original;

			StorageApplicationPermissions.FutureAccessList.Remove(ProfilesFileToken);

			SettingsStorage.DeleteObjectLocal(nameof(ProfilesFileToken));
#else
#endif
			SettingsStorage.DeleteObjectLocal(nameof(ProfilesPathLocation));

			OnPropertyChanged(nameof(ProfilesPathLocation));
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
	public enum TagsPopupLocation
	{
		[Int(12)]
		Top,
		[Int(3)]
		Middle,
		[Int(11)]
		Bottom
	}
}
