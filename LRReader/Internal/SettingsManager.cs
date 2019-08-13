using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LRReader.Internal
{
	public class SettingsManager : ViewModelBase
	{
		private ApplicationDataContainer settings;

		public bool UseRoamedSettings
		{
			get
			{
				object val = ApplicationData.Current.LocalSettings.Values["UseRoamedSettings"];
				return val != null ? (bool)val : false;
			}
			set
			{
				ApplicationData.Current.LocalSettings.Values["UseRoamedSettings"] = value;
				settings = value ? ApplicationData.Current.RoamingSettings : ApplicationData.Current.LocalSettings;
				RaisePropertyChanged(string.Empty);
			}
		}
		public string ServerAddress
		{
			get => settings.Values["ServerAddress"] as string;
			set => settings.Values["ServerAddress"] = value;
		}
		public string ServerApiKey
		{
			get => settings.Values["ServerApiKey"] as string;
			set => settings.Values["ServerApiKey"] = value;
		}
		public float BaseZoom
		{
			get
			{
				var val = settings.Values["BaseZoom"];
				return val != null ? (float)val : 1.0f;
			}
			set
			{
				settings.Values["BaseZoom"] = value;
				RaisePropertyChanged("BaseZoom");
			}
		}
		public float ZoomedFactor
		{
			get
			{
				var val = settings.Values["ZoomedFactor"];
				return val != null ? (float)val : 2.0f;
			}
			set
			{
				settings.Values["ZoomedFactor"] = value;
				RaisePropertyChanged("ZoomedFactor");
			}
		}
		public SettingsManager()
		{
			object value = ApplicationData.Current.LocalSettings.Values["UseRoamedSettings"];
			settings = (value != null ? (bool)value : false) ? ApplicationData.Current.RoamingSettings : ApplicationData.Current.LocalSettings;
		}
	}
}
