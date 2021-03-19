using LRReader.Shared.Services;
using Windows.Storage;

namespace LRReader.UWP.Services
{
	public class SettingsStorageService : ISettingsStorageService
	{
		private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		private ApplicationDataContainer roamedSettings = ApplicationData.Current.RoamingSettings;

		public T GetObjectLocal<T>(string key) => GetObjectLocal<T>(key, default);

		public T GetObjectLocal<T>(string key, T def)
		{
			var val = localSettings.Values[key];
			return val != null ? (T)val : def;
		}

		public T GetObjectRoamed<T>(string key) => GetObjectRoamed<T>(key, default);

		public T GetObjectRoamed<T>(string key, T def)
		{
			var val = roamedSettings.Values[key];
			return val != null ? (T)val : def;
		}

		public void StoreObjectLocal(string key, object obj) => localSettings.Values[key] = obj;

		public void StoreObjectRoamed(string key, object obj) => roamedSettings.Values[key] = obj;

		public void DeleteObjectLocal(string key)
		{
			localSettings.Values.Remove(key);
		}

		public void DeleteObjectRoamed(string key)
		{
			roamedSettings.Values.Remove(key);
		}
	}
}
