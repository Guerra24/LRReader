using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Newtonsoft.Json;

namespace LRReader.Shared.Internal
{
	public class ArchivesManager
	{
		public Dictionary<string, Archive> Archives = new Dictionary<string, Archive>();
		public List<TagStats> TagStats = new List<TagStats>();
		public List<string> Namespaces = new List<string>();

		public static string TemporaryFolder;

		public async Task ReloadArchives()
		{
			Archives.Clear();
			TagStats.Clear();
			Namespaces.Clear();

			var serverInfo = await ServerProvider.GetServerInfo();
			var currentTimestamp = SharedGlobal.SettingsStorage.GetObjectLocal("CacheTimestamp", -1);
			if (currentTimestamp != serverInfo.cache_last_cleared)
			{
				SharedGlobal.SettingsStorage.StoreObjectLocal("CacheTimestamp", serverInfo.cache_last_cleared);
				await Update();
			}
			else
			{
				try
				{
					var index = FilesStorage.GetFile(TemporaryFolder + "/Index-v2.json");
					var tags = FilesStorage.GetFile(TemporaryFolder + "/Tags-v1.json");
					var namespaces = FilesStorage.GetFile(TemporaryFolder + "/Namespaces-v1.json");
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
			var resultA = (await ArchivesProvider.GetArchives()).ToDictionary(c => c.arcid, c => c);
			if (resultA != null)
			{
				await FilesStorage.StoreFile(TemporaryFolder + "/Index-v2.json", JsonConvert.SerializeObject(resultA));
				Archives = resultA;
			}
			var resultT = await DatabaseProvider.GetTagStats();
			if (resultT != null)
			{
				await FilesStorage.StoreFile(TemporaryFolder + "/Tags-v1.json", JsonConvert.SerializeObject(resultT));
				foreach (var t in resultT)
				{
					if (!string.IsNullOrEmpty(t.@namespace) && !Namespaces.Exists(s => s.Equals(t.@namespace)))
						Namespaces.Add(t.@namespace);
					TagStats.Add(t);
				}
				await FilesStorage.StoreFile(TemporaryFolder + "/Namespaces-v1.json", JsonConvert.SerializeObject(Namespaces));
			}
		}

		public Archive GetArchive(string id)
		{
			if (Archives.TryGetValue(id, out Archive archive))
				return archive;
			return null;
		}

	}
}
