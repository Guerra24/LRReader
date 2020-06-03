using GalaSoft.MvvmLight;
using LRReader.Shared.Models.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRReader.Shared.Internal.SharedGlobal;

namespace LRReader.Shared.Internal
{
	public class SettingsManager : ViewModelBase
	{

		private ObservableCollection<ServerProfile> _profiles;
		public ObservableCollection<ServerProfile> Profiles
		{
			get => _profiles;
			set
			{
				_profiles = value;
				RaisePropertyChanged("Profiles");
			}
		}
		private ServerProfile _profile;
		public ServerProfile Profile
		{
			get => _profile;
			set
			{
				if (value != null)
					SettingsStorage.StoreObjectLocal("ProfileUID", value.UID);
				if (_profile != value)
				{
					_profile = value;
					RaisePropertyChanged("Profile");
				}
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
				RaisePropertyChanged("DefaultZoom");
			}
		}
		public bool ImageCaching
		{
			get => SettingsStorage.GetObjectLocal("ImageCaching", false);
			set
			{
				SettingsStorage.StoreObjectLocal("ImageCaching", value);
				RaisePropertyChanged("ImageCaching");
			}
		}
		public bool ReadRTL
		{
			get => SettingsStorage.GetObjectLocal("ReadRTL", false);
			set
			{
				SettingsStorage.StoreObjectLocal("ReadRTL", value);
				RaisePropertyChanged("ReadRTL");
				EventManager.RebuildReaderImagesSet();
			}
		}
		public bool TwoPages
		{
			get => SettingsStorage.GetObjectLocal("TwoPages", false);
			set
			{
				SettingsStorage.StoreObjectLocal("TwoPages", value);
				RaisePropertyChanged("TwoPages");
				EventManager.RebuildReaderImagesSet();
			}
		}
		public bool BookmarkReminder
		{
			get => SettingsStorage.GetObjectRoamed("BookmarkReminder", true);
			set
			{
				SettingsStorage.StoreObjectRoamed("BookmarkReminder", value);
				RaisePropertyChanged("BookmarkReminder");
			}
		}
		public BookmarkReminderMode BookmarkReminderMode
		{
			get => (BookmarkReminderMode)SettingsStorage.GetObjectRoamed("BookmarkReminderMode", (int)BookmarkReminderMode.New);
			set
			{
				SettingsStorage.StoreObjectRoamed("BookmarkReminderMode", (int)value);
			}
		}
		public bool RemoveBookmark
		{
			get => SettingsStorage.GetObjectRoamed("RemoveBookmark", true);
			set
			{
				SettingsStorage.StoreObjectRoamed("RemoveBookmark", value);
				RaisePropertyChanged("RemoveBookmark");
			}
		}
		public bool OpenBookmarksTab
		{
			get => SettingsStorage.GetObjectRoamed("OpenBookmarksTab", true);
			set
			{
				SettingsStorage.StoreObjectRoamed("OpenBookmarksTab", value);
				RaisePropertyChanged("OpenBookmarksTab");
			}
		}
		public bool OpenBookmarksStart
		{
			get => SettingsStorage.GetObjectRoamed("OpenBookmarksStart", false);
			set
			{
				SettingsStorage.StoreObjectRoamed("OpenBookmarksStart", value);
				RaisePropertyChanged("OpenBookmarksStart");
			}
		}
		public bool OpenReader
		{
			get => SettingsStorage.GetObjectRoamed("OpenReader", false);
			set
			{
				SettingsStorage.StoreObjectRoamed("OpenReader", value);
				RaisePropertyChanged("OpenReader");
			}
		}
		public int KeyboardScroll
		{
			get => SettingsStorage.GetObjectLocal("KeyboardScroll", 200);
			set
			{
				SettingsStorage.StoreObjectLocal("KeyboardScroll", value);
				RaisePropertyChanged("KeyboardScroll");
			}
		}
		public bool FitToWidth
		{
			get => SettingsStorage.GetObjectRoamed("FitToWidth", false);
			set
			{
				SettingsStorage.StoreObjectRoamed("FitToWidth", value);
				RaisePropertyChanged("FitToWidth");
			}
		}
		public int FitScaleLimit
		{
			get => SettingsStorage.GetObjectLocal("FitScaleLimit", 100);
			set
			{
				SettingsStorage.StoreObjectLocal("FitScaleLimit", value);
				RaisePropertyChanged("FitScaleLimit");
			}
		}
		public AppTheme Theme
		{
			get => (AppTheme)SettingsStorage.GetObjectLocal("Theme", (int)AppTheme.System);
			set
			{
				SettingsStorage.StoreObjectLocal("Theme", (int)value);
			}
		}
		public bool CompactBookmarks
		{
			get => SettingsStorage.GetObjectRoamed("CompactBookmarks", true);
			set
			{
				SettingsStorage.StoreObjectRoamed("CompactBookmarks", value);
			}
		}
		public bool OpenCategoriesTab
		{
			get => SettingsStorage.GetObjectRoamed("OpenCategoriesTab", true);
			set
			{
				SettingsStorage.StoreObjectRoamed("OpenCategoriesTab", value);
			}
		}
		public static readonly int CurrentLocalVersion = 3;
		public int SettingsVersionLocal
		{
			get => SettingsStorage.GetObjectLocal<int>("SettingsVersion");
			set
			{
				SettingsStorage.StoreObjectLocal("SettingsVersion", value);
				RaisePropertyChanged("SettingsVersionLocal");
			}
		}
		public static readonly int CurrentRoamedVersion = 1;
		public int SettingsVersionRoamed
		{
			get => SettingsStorage.GetObjectRoamed<int>("SettingsVersion");
			set
			{
				SettingsStorage.StoreObjectRoamed("SettingsVersion", value);
				RaisePropertyChanged("SettingsVersionRoamed");
			}
		}
		public SettingsManager()
		{
			var profiles = SettingsStorage.GetObjectRoamed<string>("Profiles");
			if (profiles != null)
			{
				Profiles = JsonConvert.DeserializeObject<ObservableCollection<ServerProfile>>(profiles as string);
			}
			else
			{
				Profiles = new ObservableCollection<ServerProfile>();
			}

			UpgradeProfiles();

			SaveProfiles();

			UpgradeSettings();

			Profiles.CollectionChanged += ProfilesChanges;

			var profile = SettingsStorage.GetObjectLocal<string>("ProfileUID");
			if (profile != null)
			{
				Profile = Profiles.FirstOrDefault(p => p.UID.Equals(profile as string));
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
			RaisePropertyChanged("ProfilesAvailable");
			RaisePropertyChanged("AtLeastOneProfile");
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
			SettingsStorage.StoreObjectRoamed("Profiles", JsonConvert.SerializeObject(Profiles));
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
