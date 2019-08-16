using GalaSoft.MvvmLight;
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
			Profiles.CollectionChanged += ProfilesChanges;

			var profile = localSettings.Values["ProfileUID"];
			if (profile != null)
			{
				Profile = Profiles.FirstOrDefault(p => p.UID.Equals(profile as string));
			}
		}

		private void ProfilesChanges(object sender, NotifyCollectionChangedEventArgs e)
		{
			roamedSettings.Values["Profiles"] = JsonConvert.SerializeObject(Profiles);
			RaisePropertyChanged("ProfilesAvailable");
			RaisePropertyChanged("AtLeastOneProfile");
		}

		public void AddProfile(string name, string address, string apikey)
		{
			ServerProfile profile = new ServerProfile();
			profile.Name = name;
			profile.ServerAddress = address;
			profile.ServerApiKey = apikey;
			Profiles.Add(profile);
		}

		public void ModifyProfile(string uid, string name, string address, string apikey)
		{
			var profile = Profiles.FirstOrDefault(p => p.UID.Equals(uid));
			profile.Name = name;
			profile.ServerAddress = address;
			profile.ServerApiKey = apikey;
			profile.Update();
		}
	}
	public class ServerProfile : ObservableObject
	{
		public string UID { get; set; }
		public string Name { get; set; }
		public string ServerAddress { get; set; }
		public string ServerApiKey { get; set; }

		public ServerProfile()
		{
			UID = Guid.NewGuid().ToString();
		}

		public void Update()
		{
			RaisePropertyChanged(string.Empty);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
