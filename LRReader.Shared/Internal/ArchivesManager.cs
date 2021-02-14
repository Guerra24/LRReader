using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Newtonsoft.Json;

namespace LRReader.Shared.Internal
{
	public class ArchivesManager
	{
		public List<Archive> Archives = new List<Archive>();
		public List<TagStats> TagStats = new List<TagStats>();
		public List<string> Namespaces = new List<string>();

		public static string TemporaryFolder;

		public async Task ReloadArchives()
		{
			Archives.Clear();
			TagStats.Clear();
			Namespaces.Clear();

			var serverInfo = await ServerProvider.GetServerInfo();

			if (serverInfo.version >= new Version(0, 7, 3))
			{
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
						var index = SharedGlobal.FilesStorage.GetFile(TemporaryFolder + "/Index.json");
						var tags = SharedGlobal.FilesStorage.GetFile(TemporaryFolder + "/Tags.json");
						var namespaces = SharedGlobal.FilesStorage.GetFile(TemporaryFolder + "/Namespaces.json");
						Archives = JsonConvert.DeserializeObject<List<Archive>>(await index);
						TagStats = JsonConvert.DeserializeObject<List<TagStats>>(await tags);
						Namespaces = JsonConvert.DeserializeObject<List<string>>(await namespaces);
					}
					catch (Exception)
					{
						await Update();
					}
				}
			}
			else
			{
				await Update();
			}
		}

		private async Task Update()
		{
			var resultA = await ArchivesProvider.GetArchives();
			if (resultA != null)
			{
				await SharedGlobal.FilesStorage.StoreFile(TemporaryFolder + "/Index.json", JsonConvert.SerializeObject(resultA));
				Archives = resultA;
			}
			var resultT = await DatabaseProvider.GetTagStats();
			if (resultT != null)
			{
				await SharedGlobal.FilesStorage.StoreFile(TemporaryFolder + "/Tags.json", JsonConvert.SerializeObject(resultT));
				foreach (var t in resultT)
				{
					if (!string.IsNullOrEmpty(t.@namespace) && !Namespaces.Exists(s => s.Equals(t.@namespace)))
						Namespaces.Add(t.@namespace);
					TagStats.Add(t);
				}
				await SharedGlobal.FilesStorage.StoreFile(TemporaryFolder + "/Namespaces.json", JsonConvert.SerializeObject(Namespaces));
			}
		}

	}
}
