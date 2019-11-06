using GalaSoft.MvvmLight;
using LRReader.Shared.Models.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LRReader.Internal
{
	public class SettingsManager : ViewModelBase
	{
		private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		private ApplicationDataContainer roamedSettings = ApplicationData.Current.RoamingSettings;

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
					localSettings.Values["ProfileUID"] = value.UID;
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
		public float BaseZoom
		{
			get
			{
				var val = localSettings.Values["BaseZoom"];
				return val != null ? (float)val : 1.0f;
			}
			set
			{
				localSettings.Values["BaseZoom"] = value;
				RaisePropertyChanged("BaseZoom");
			}
		}
		public float ZoomedFactor
		{
			get
			{
				var val = localSettings.Values["ZoomedFactor"];
				return val != null ? (float)val : 2.0f;
			}
			set
			{
				localSettings.Values["ZoomedFactor"] = value;
				RaisePropertyChanged("ZoomedFactor");
			}
		}
		public bool ImageCaching
		{
			get
			{
				var val = localSettings.Values["ImageCaching"];
				return val != null ? (bool)val : false;
			}
			set
			{
				localSettings.Values["ImageCaching"] = value;
				RaisePropertyChanged("ImageCaching");
			}
		}
		public bool ReadRTL
		{
			get
			{
				var val = localSettings.Values["ReadRTL"];
				return val != null ? (bool)val : false;
			}
			set
			{
				localSettings.Values["ReadRTL"] = value;
				RaisePropertyChanged("ReadRTL");
				Global.EventManager.RebuildReaderImagesSet();
			}
		}
		public bool TwoPages
		{
			get
			{
				var val = localSettings.Values["TwoPages"];
				return val != null ? (bool)val : false;
			}
			set
			{
				localSettings.Values["TwoPages"] = value;
				RaisePropertyChanged("TwoPages");
				Global.EventManager.RebuildReaderImagesSet();
			}
		}
		public bool SwitchTabArchive
		{
			get
			{
				var val = localSettings.Values["SwitchTabArchive"];
				return val != null ? (bool)val : true;
			}
			set
			{
				localSettings.Values["SwitchTabArchive"] = value;
				RaisePropertyChanged("SwitchTabArchive");
			}
		}
		public bool BookmarkReminder
		{
			get
			{
				var val = roamedSettings.Values["BookmarkReminder"];
				return val != null ? (bool)val : true;
			}
			set
			{
				roamedSettings.Values["BookmarkReminder"] = value;
				RaisePropertyChanged("BookmarkReminder");
			}
		}
		public bool RemoveBookmark
		{
			get
			{
				var val = roamedSettings.Values["RemoveBookmark"];
				return val != null ? (bool)val : true;
			}
			set
			{
				roamedSettings.Values["RemoveBookmark"] = value;
				RaisePropertyChanged("RemoveBookmark");
			}
		}
		public bool OpenBookmarksTab
		{
			get
			{
				var val = roamedSettings.Values["OpenBookmarksTab"];
				return val != null ? (bool)val : true;
			}
			set
			{
				roamedSettings.Values["OpenBookmarksTab"] = value;
				RaisePropertyChanged("OpenBookmarksTab");
			}
		}
		public bool OpenBookmarksStart
		{
			get
			{
				var val = roamedSettings.Values["OpenBookmarksStart"];
				return val != null ? (bool)val : false;
			}
			set
			{
				roamedSettings.Values["OpenBookmarksStart"] = value;
				RaisePropertyChanged("OpenBookmarksStart");
			}
		}
		public SettingsManager()
		{
			var profiles = roamedSettings.Values["Profiles"];
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

			Profiles.CollectionChanged += ProfilesChanges;

			var profile = localSettings.Values["ProfileUID"];
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
			roamedSettings.Values["Profiles"] = JsonConvert.SerializeObject(Profiles);
		}
	}
}
