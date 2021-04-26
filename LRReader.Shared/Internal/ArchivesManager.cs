using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Internal
{
	public class ArchivesManager
	{
		public Dictionary<string, Archive> Archives = new Dictionary<string, Archive>();
		public List<TagStats> TagStats = new List<TagStats>();
		public List<string> Namespaces = new List<string>();

		public async Task ReloadArchives()
		{
			Archives.Clear();
			TagStats.Clear();
			Namespaces.Clear();

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
					Archives = JsonConvert.DeserializeObject<Dictionary<string, Archive>>(await index);
					TagStats = JsonConvert.DeserializeObject<List<TagStats>>(await tags);
					Namespaces = JsonConvert.DeserializeObject<List<string>>(await namespaces);
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
		}

		public Archive GetArchive(string id)
		{
			if (Archives.TryGetValue(id, out Archive archive))
				return archive;
			return null;
		}

		public async Task DeleteArchive(string id)
		{
			if (!Archives.ContainsKey(id))
				return;
			var result = await ArchivesProvider.DeleteArchive(id);
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
				Events.ShowNotification("An error ocurred while deleting archive.", "Metadata has been deleted, remove file manually.", 0);
			}
		}

	}
}
