#nullable enable
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LRReader.Shared.Services;
using Windows.Storage;

namespace LRReader.UWP.Services
{
	public class SettingsStorageService : ISettingsStorageService
	{
		private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
		private ApplicationDataContainer roamedSettings = ApplicationData.Current.RoamingSettings;

		public Task Init() => Task.Delay(1);

		public T? GetObjectLocal<T>([CallerMemberName] string? key = null) => GetObjectLocal<T>(default, key);

		//[return: NotNullIfNotNull("def")]
		public T? GetObjectLocal<T>(T? def, [CallerMemberName] string? key = null)
		{
			var val = localSettings.Values[key];
			return val != null ? (T)val : def;
		}

		public T? GetObjectRoamed<T>([CallerMemberName] string? key = null) => GetObjectRoamed<T>(default, key);

		//[return: NotNullIfNotNull("def")]
		public T? GetObjectRoamed<T>(T? def, [CallerMemberName] string? key = null)
		{
			var val = roamedSettings.Values[key];
			return val != null ? (T)val : def;
		}

		public void StoreObjectLocal(object obj, [CallerMemberName] string? key = null) => localSettings.Values[key] = obj;

		public void StoreObjectRoamed(object obj, [CallerMemberName] string? key = null) => roamedSettings.Values[key] = obj;

		public void DeleteObjectLocal(string key) => localSettings.Values.Remove(key);

		public void DeleteObjectRoamed(string key) => roamedSettings.Values.Remove(key);

		public bool ExistLocal(string key) => localSettings.Values.ContainsKey(key);

		public bool ExistRoamed(string key) => roamedSettings.Values.ContainsKey(key);
	}
}
