using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ArchivesService
	{
		private readonly IFilesService Files;
		private readonly ISettingsStorageService SettingsStorage;
		private readonly SettingsService Settings;
		private readonly EventsService Events;
		private readonly TabsService Tabs;

		public Dictionary<string, Archive> Archives = new Dictionary<string, Archive>();
		public List<TagStats> TagStats = new List<TagStats>();
		public List<string> Namespaces = new List<string>();
		public Dictionary<string, Category> Categories = new Dictionary<string, Category>();

		public ArchivesService(IFilesService files, ISettingsStorageService settingsStorage, SettingsService settings, EventsService events, TabsService tabs)
		{
			Files = files;
			SettingsStorage = settingsStorage;
			Settings = settings;
			Events = events;
			Tabs = tabs;
		}

		public async Task ReloadArchives()
		{
			Archives.Clear();
			TagStats.Clear();
			Namespaces.Clear();
			Categories.Clear();

			var serverInfo = await ServerProvider.GetServerInfo();
			if (serverInfo == null)
				return;
			var currentTimestamp = SettingsStorage.GetObjectLocal("CacheTimestamp", -1);
			if (currentTimestamp != serverInfo.cache_last_cleared)
			{
				SettingsStorage.StoreObjectLocal("CacheTimestamp", serverInfo.cache_last_cleared);
				await Update();
			}
			else
			{
				try
				{
					var index = Files.GetFile(Files.LocalCache + "/Index-v2.json");
					var tags = Files.GetFile(Files.LocalCache + "/Tags-v1.json");
					var namespaces = Files.GetFile(Files.LocalCache + "/Namespaces-v1.json");
					var categories = Files.GetFile(Files.LocalCache + "/Categories-v1.json");
					Archives = JsonConvert.DeserializeObject<Dictionary<string, Archive>>(await index);
					TagStats = JsonConvert.DeserializeObject<List<TagStats>>(await tags);
					Namespaces = JsonConvert.DeserializeObject<List<string>>(await namespaces);
					Categories = JsonConvert.DeserializeObject<Dictionary<string, Category>>(await categories);
				}
				catch (Exception)
				{
					await Update();
				}
			}
		}

		private async Task Update()
		{

			var resultA = await ArchivesProvider.GetArchives();
			if (resultA != null)
			{
				var temp = resultA.ToDictionary(c => c.arcid, c => c);
				await Files.StoreFile(Files.LocalCache + "/Index-v2.json", JsonConvert.SerializeObject(temp));
				Archives = temp;
			}
			var resultT = await DatabaseProvider.GetTagStats();
			if (resultT != null)
			{
				await Files.StoreFile(Files.LocalCache + "/Tags-v1.json", JsonConvert.SerializeObject(resultT));
				foreach (var t in resultT)
				{
					if (!string.IsNullOrEmpty(t.@namespace) && !Namespaces.Exists(s => s.Equals(t.@namespace)))
						Namespaces.Add(t.@namespace);
					TagStats.Add(t);
				}
				await Files.StoreFile(Files.LocalCache + "/Namespaces-v1.json", JsonConvert.SerializeObject(Namespaces));
			}
			var resultC = await CategoriesProvider.GetCategories();
			if (resultC != null)
			{
				var temp = resultC.ToDictionary(c => c.id, c => c);
				await Files.StoreFile(Files.LocalCache + "/Categories-v1.json", JsonConvert.SerializeObject(temp));
				Categories = temp;
			}
		}

		public Archive GetArchive(string id)
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
				Events.ShowNotification("Unable to delete archive", "", 0);
				return false;
			}
			if (result.success)
			{
				var bookmark = Settings.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(id));
				if (bookmark != null)
					Settings.Profile.Bookmarks.Remove(bookmark);
				Events.DeleteArchive(id);
				Archives.Remove(id);
			}
			else
			{
				Events.ShowNotification("An error ocurred while deleting archive", "Metadata has been deleted, remove file manually.", 0);
			}
			return true;
		}

	}
}
