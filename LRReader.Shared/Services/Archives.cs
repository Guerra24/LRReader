using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Newtonsoft.Json;

namespace LRReader.Shared.Services
{
	public class ArchivesService
	{
		private readonly IFilesService Files;
		private readonly ISettingsStorageService SettingsStorage;
		private readonly SettingsService Settings;
		private readonly TabsService Tabs;
		private readonly ApiService Api;

		public Dictionary<string, Archive> Archives { get; private set; } = new();
		public List<TagStats> TagStats { get; private set; } = new();
		public List<string> Namespaces { get; private set; } = new();
		//		public Dictionary<string, Category> Categories = new Dictionary<string, Category>();

		private string MetadataPath = "";

		private DirectoryInfo metadataDirectory;

		public ArchivesService(IFilesService files, ISettingsStorageService settingsStorage, SettingsService settings, TabsService tabs, ApiService api)
		{
			Files = files;
			SettingsStorage = settingsStorage;
			Settings = settings;
			Tabs = tabs;
			Api = api;
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

			var profile = Settings.Profile;

			var currentTimestamp = profile.CacheTimestamp;
			MetadataPath = $"{metadataDirectory.FullName}/{profile.UID}";

			SettingsStorage.DeleteObjectLocal("CacheTimestamp");

			if (currentTimestamp != serverInfo.cache_last_cleared || !Directory.Exists(MetadataPath) || Api.ControlFlags.BrokenCache)
			{
				profile.CacheTimestamp = serverInfo.cache_last_cleared;
				Settings.SaveProfiles();
				await Update(MetadataPath);
			}
			else
			{
				try
				{
					var index = Files.GetFile($"{MetadataPath}/Index-v3.json");
					var tags = Files.GetFile($"{MetadataPath}/Tags-v1.json");
					var namespaces = Files.GetFile($"{MetadataPath}/Namespaces-v1.json");
					var categories = Files.GetFile($"{MetadataPath}/Categories-v1.json");
					Archives = JsonConvert.DeserializeObject<Dictionary<string, Archive>>(await index) ?? new();
					TagStats = JsonConvert.DeserializeObject<List<TagStats>>(await tags) ?? new();
					Namespaces = JsonConvert.DeserializeObject<List<string>>(await namespaces) ?? new();
					//Categories = JsonConvert.DeserializeObject<Dictionary<string, Category>>(await categories);
				}
				catch (Exception)
				{
					await Update(MetadataPath);
				}
			}
		}

		private async Task Update(string path)
		{
			Directory.CreateDirectory(path);
			var resultATask = ArchivesProvider.GetArchives();
			var resultTTask = DatabaseProvider.GetTagStats();
			await Task.WhenAll(resultATask, resultTTask);
			var resultA = resultATask.Result;
			var resultT = resultTTask.Result;

			if (resultA != null)
			{
				var temp = resultA.ToDictionary(c => c.arcid, c => c);
				await Files.StoreFile($"{path}/Index-v3.json", JsonConvert.SerializeObject(temp));
				Archives = temp;
			}
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
			if (Archives.TryGetValue(id, out var archive))
				return archive;
			return null;
		}

		public bool HasArchive(string id) => Archives.ContainsKey(id);

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
				Settings.Profile.Bookmarks.RemoveAll(b => b.archiveID.Equals(id));
				Settings.Profile.MarkedAsNonDuplicated.RemoveAll(hit => hit.Left.Equals(id) || hit.Right.Equals(id));
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
