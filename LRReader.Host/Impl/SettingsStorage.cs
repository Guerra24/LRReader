using LRReader.Shared.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LRReader.Host.Impl
{
	public class SettingsStorage : ISettingsStorage
	{

		private Dictionary<string, object> roamedSettings = new Dictionary<string, object>();
		private Dictionary<string, object> localSettings = new Dictionary<string, object>();

		private const string FileRoamed = "RoamedSettings.json";
		private const string FileLocal = "LocalSettings.json";

		private string AppDataDir;
		private string AppDataFile;
		private string LocalDataDir;
		private string LocalDataFile;

		public SettingsStorage()
		{
			AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LRReader");
			AppDataFile = Path.Combine(AppDataDir, FileRoamed);
			LocalDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LRReader");
			LocalDataFile = Path.Combine(LocalDataDir, FileLocal);
		}

		public void Load()
		{
			if (File.Exists(AppDataFile))
				roamedSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(AppDataFile));
			if (File.Exists(LocalDataFile))
				localSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(LocalDataFile));
		}

		public void Save()
		{
			if (!Directory.Exists(AppDataDir))
				Directory.CreateDirectory(AppDataDir);
			if (!Directory.Exists(LocalDataDir))
				Directory.CreateDirectory(LocalDataDir);
			File.WriteAllText(AppDataFile, JsonConvert.SerializeObject(roamedSettings));
			File.WriteAllText(LocalDataFile, JsonConvert.SerializeObject(localSettings));
		}

		public T GetObjectLocal<T>(string key) => GetObjectLocal<T>(key, default);

		public T GetObjectLocal<T>(string key, T def)
		{
			var val = localSettings[key];
			return val != null ? (T)val : def;
		}

		public T GetObjectRoamed<T>(string key) => GetObjectRoamed<T>(key, default);

		public T GetObjectRoamed<T>(string key, T def)
		{
			var val = roamedSettings[key];
			return val != null ? (T)val : def;
		}

		public void StoreObjectLocal(string key, object obj) => localSettings[key] = obj;

		public void StoreObjectRoamed(string key, object obj) => roamedSettings[key] = obj;
	}
}
