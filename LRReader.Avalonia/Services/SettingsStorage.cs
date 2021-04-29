using LRReader.Shared.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{
	public class SettingsStorageService : ISettingsStorageService
	{

		private readonly IFilesService Files;

		private Dictionary<string, object> roamedSettings = new Dictionary<string, object>();
		private Dictionary<string, object> localSettings = new Dictionary<string, object>();

		private const string FileRoamed = "RoamedSettings.json";
		private const string FileLocal = "LocalSettings.json";

		private string AppDataFile;
		private string LocalDataFile;

		public SettingsStorageService(IFilesService files)
		{
			Files = files;
			AppDataFile = Path.Combine(Files.Local, FileRoamed);
			LocalDataFile = Path.Combine(Files.LocalCache, FileLocal);
		}

		public async Task Init()
		{
			if (File.Exists(AppDataFile))
				roamedSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(await File.ReadAllTextAsync(AppDataFile));
			if (File.Exists(LocalDataFile))
				localSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(await File.ReadAllTextAsync(LocalDataFile));
		}

		public void Save()
		{
			File.WriteAllText(AppDataFile, JsonConvert.SerializeObject(roamedSettings));
			File.WriteAllText(LocalDataFile, JsonConvert.SerializeObject(localSettings));
		}

		public T GetObjectLocal<T>(string key) => GetObjectLocal<T>(key, default);

		public T GetObjectLocal<T>(string key, T def)
		{
			if (!localSettings.ContainsKey(key))
				return def;
			var val = localSettings[key];
			return val != null ? (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture) : def;
		}

		public T GetObjectRoamed<T>(string key) => GetObjectRoamed<T>(key, default);

		public T GetObjectRoamed<T>(string key, T def)
		{
			if (!roamedSettings.ContainsKey(key))
				return def;
			var val = roamedSettings[key];
			return val != null ? (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture) : def;
		}

		public void StoreObjectLocal(string key, object obj)
		{
			localSettings[key] = obj;
			Save();
		}

		public void StoreObjectRoamed(string key, object obj)
		{
			roamedSettings[key] = obj;
			Save();
		}

		public void DeleteObjectLocal(string key)
		{
			localSettings.Remove(key);
			Save();
		}

		public void DeleteObjectRoamed(string key)
		{
			roamedSettings.Remove(key);
			Save();
		}
	}
}
