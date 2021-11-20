using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Microsoft.Toolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ArchivesService
	{
		private readonly IFilesService Files;
		private readonly ISettingsStorageService SettingsStorage;
		private readonly SettingsService Settings;
		private readonly TabsService Tabs;

		public Dictionary<string, Archive> Archives = new Dictionary<string, Archive>();
		public List<TagStats> TagStats = new List<TagStats>();
		public List<string> Namespaces = new List<string>();
		//		public Dictionary<string, Category> Categories = new Dictionary<string, Category>();

		private DirectoryInfo metadataDirectory;

		public ArchivesService(IFilesService files, ISettingsStorageService settingsStorage, SettingsService settings, TabsService tabs)
		{
			Files = files;
			SettingsStorage = settingsStorage;
			Settings = settings;
			Tabs = tabs;
			metadataDirectory = Directory.CreateDirectory(Files.LocalCache + "/Metadata");
		}

		public async Task ReloadArchives()
		{
			Archives.Clear();
			TagStats.Clear();
			Namespaces.Clear();
			//Categories.Clear();
			foreach (var json in Directory.GetFiles(Files.LocalCache, "*.json", SearchOption.TopDirectoryOnly))
				File.Delete(json);

			var serverInfo = await ServerProvider.GetServerInfo();
			if (serverInfo == null)
				return;

			var path = $"{metadataDirectory.FullName}/{Settings.Profile.UID}";
			var currentTimestamp = SettingsStorage.GetObjectLocal("CacheTimestamp", -1);
			if (currentTimestamp != serverInfo.cache_last_cleared || !Directory.Exists(path))
			{
				SettingsStorage.StoreObjectLocal("CacheTimestamp", serverInfo.cache_last_cleared);
				await Update(path);
			}
			else
			{
				try
				{
					var index = Files.GetFile($"{path}/Index-v3.json");
					var tags = Files.GetFile($"{path}/Tags-v1.json");
					var namespaces = Files.GetFile($"{path}/Namespaces-v1.json");
					var categories = Files.GetFile($"{path}/Categories-v1.json");
					Archives = JsonConvert.DeserializeObject<Dictionary<string, Archive>>(await index);
					TagStats = JsonConvert.DeserializeObject<List<TagStats>>(await tags);
					Namespaces = JsonConvert.DeserializeObject<List<string>>(await namespaces);
					//Categories = JsonConvert.DeserializeObject<Dictionary<string, Category>>(await categories);
				}
				catch (Exception)
				{
					await Update(path);
				}
			}
		}

		private async Task Update(string path)
		{
			Directory.CreateDirectory(path);
			var resultA = await ArchivesProvider.GetArchives();
			if (resultA != null)
			{
				var temp = resultA.ToDictionary(c => c.arcid, c => c);
				await Files.StoreFile($"{path}/Index-v3.json", JsonConvert.SerializeObject(temp));
				Archives = temp;
			}
			var resultT = await DatabaseProvider.GetTagStats();
			if (resultT != null)
			{
				await Files.StoreFile($"{path}/Tags-v1.json", JsonConvert.SerializeObject(resultT));
				foreach (var t in resultT)
				{
					if (!string.IsNullOrEmpty(t.@namespace) && !Namespaces.Exists(s => s.Equals(t.@namespace)))
						Namespaces.Add(t.@namespace);
					TagStats.Add(t);
				}
				await Files.StoreFile($"{path}/Namespaces-v1.json", JsonConvert.SerializeObject(Namespaces));
			}
			/*var resultC = await CategoriesProvider.GetCategories();
			if (resultC != null)
			{
				var temp = resultC.ToDictionary(c => c.id, c => c);
				await Files.StoreFile($"{path}/Categories-v1.json", JsonConvert.SerializeObject(temp));
				Categories = temp;
			}*/
		}

		public Archive? GetArchive(string id)
		{
			if (Archives.TryGetValue(id, out Archive archive))
				return archive;
			return null;
		}

		public void OpenTab(Archive archive, bool switchToTab = true, IList<Archive>? next = null)
		{
			Tabs.OpenTab(Tab.Archive, switchToTab, archive, next);
		}

		public async Task<bool> DeleteArchive(string id)
		{
			if (!Archives.ContainsKey(id))
				return false;
			var result = await ArchivesProvider.DeleteArchive(id);
			if (result == null)
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("Unable to delete archive", "", 0));
				return false;
			}
			if (result.success)
			{
				var bookmark = Settings.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(id));
				if (bookmark != null)
					Settings.Profile.Bookmarks.Remove(bookmark);
				WeakReferenceMessenger.Default.Send(new DeleteArchiveMessage(Archives[id]));
				Tabs.CloseTabWithId("Edit_" + id);
				Tabs.CloseTabWithId("Archive_" + id);
				Archives.Remove(id);
			}
			else
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("An error ocurred while deleting archive", "Metadata has been deleted, remove file manually.", 0));
			}
			return true;
		}

	}
}
