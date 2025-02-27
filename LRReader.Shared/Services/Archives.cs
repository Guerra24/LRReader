using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ArchivesService
	{
		private readonly IFilesService Files;
		private readonly ISettingsStorageService SettingsStorage;
		private readonly SettingsService Settings;
		private readonly TabsService Tabs;
		private readonly ApiService Api;

		public ConcurrentDictionary<string, Archive> Archives { get; private set; } = new();
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
			await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
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
					var index = Files.GetFile($"{MetadataPath}/Index-v4.json");
					var tags = Files.GetFile($"{MetadataPath}/Tags-v2.json");
					var namespaces = Files.GetFile($"{MetadataPath}/Namespaces-v2.json");
					var categories = Files.GetFile($"{MetadataPath}/Categories-v2.json");
					Archives = JsonSerializer.Deserialize<ConcurrentDictionary<string, Archive>>(await index, JsonSettings.Options) ?? new();
					TagStats = JsonSerializer.Deserialize<List<TagStats>>(await tags, JsonSettings.Options) ?? new();
					Namespaces = JsonSerializer.Deserialize<List<string>>(await namespaces, JsonSettings.Options) ?? new();
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

			if (!Settings.UseIncrementalCaching)
			{
				var archives = await ArchivesProvider.GetArchives();
				if (archives != null)
				{
					var temp = new ConcurrentDictionary<string, Archive>(archives.ToDictionary(c => c.arcid, c => c));
					await Files.StoreFile($"{path}/Index-v4.json", JsonSerializer.Serialize(temp, JsonSettings.Options));
					Archives = temp;
				}
			}

			var tagStats = await DatabaseProvider.GetTagStats();
			if (tagStats != null)
			{
				await Files.StoreFile($"{path}/Tags-v2.json", JsonSerializer.Serialize(tagStats, JsonSettings.Options));
				foreach (var t in tagStats)
				{
					if (!string.IsNullOrEmpty(t.@namespace) && !Namespaces.Exists(s => s.Equals(t.@namespace)))
						Namespaces.Add(t.@namespace);
					TagStats.Add(t);
				}
				await Files.StoreFile($"{path}/Namespaces-v2.json", JsonSerializer.Serialize(Namespaces, JsonSettings.Options));
			}
			/*var resultC = await CategoriesProvider.GetCategories();
			if (resultC != null)
			{
				var temp = resultC.ToDictionary(c => c.id, c => c);
				await Files.StoreFile($"{path}/Categories-v1.json", JsonConvert.SerializeObject(temp));
				Categories = temp;
			}*/
		}

		public async Task<Archive?> GetOrAddArchive(string id)
		{
			var archive = GetArchive(id);
			if (Settings.UseIncrementalCaching && archive == null)
			{
				archive = await ArchivesProvider.GetArchive(id);
				if (archive != null)
					Archives.TryAdd(id, archive);
			}
			return archive;
		}

		public Archive? GetArchive(string id)
		{
			if (Archives.TryGetValue(id, out var archive))
				return archive;
			return null;
		}

		public bool TryGetArchive(string id, [MaybeNullWhen(false)] out Archive archive) => Archives.TryGetValue(id, out archive);

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
				WeakReferenceMessenger.Default.Send(new ShowNotification("Unable to delete archive", "", 0, NotificationSeverity.Error));
				return false;
			}
			if (result.success)
			{
				Settings.Profile.Bookmarks.RemoveAll(b => b.archiveID.Equals(id));
				Settings.Profile.MarkedAsNonDuplicated.RemoveAll(hit => hit.Left.Equals(id) || hit.Right.Equals(id));
				WeakReferenceMessenger.Default.Send(new DeleteArchiveMessage(Archives[id]));
				Tabs.CloseTabWithId("Edit_" + id);
				Tabs.CloseTabWithId("Archive_" + id);
				Archives.TryRemove(id, out var _);
			}
			else
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("An error ocurred while deleting archive", "Metadata has been deleted, remove file manually.", 0, NotificationSeverity.Warning));
			}
			return true;
		}

	}
}
