using CommunityToolkit.Mvvm.ComponentModel;
using LRReader.Shared.Models;
using Sentry;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LRReader.Shared.Services;

[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
public partial class SessionService : ObservableObject
{
	private readonly IFilesService Files;
	private readonly TabsService Tabs;
	private readonly SettingsService Settings;
	private readonly ArchivesService Archives;

	private readonly string SessionDir;

	[ObservableProperty]
	private bool _showRestore;

	public SessionService(IFilesService files, TabsService tabs, SettingsService settings, ArchivesService archives)
	{
		Files = files;
		Tabs = tabs;
		Settings = settings;
		Archives = archives;
		SessionDir = Path.Combine(Files.Local, "Session");
		Directory.CreateDirectory(SessionDir);
	}

	public async Task Suspend()
	{
		if (Settings.Profile != null)
		{
			var path = Path.Combine(SessionDir, $"{Settings.Profile.UID}.json");

			if (File.Exists(path))
				File.Copy(path, $"{path}.old", true);

			var appState = new AppState();
			foreach (var tab in Tabs.TabItems)
				appState.Tabs.Add(tab.GetTabState());

			await Files.StoreFileSafe(path, JsonSerializer.Serialize(appState, JsonSettings.Options));
		}
	}

	public async Task<bool> HasValidTabs()
	{
		var appState = await LoadSession();
		if (appState == null)
			return false;
		var valid = new Tab[] { Tab.Archive, Tab.ArchiveEdit, Tab.SearchResults };
		return appState.Tabs.Any(tab => valid.Contains(tab.Tab));
	}

	public async Task Restore()
	{
		var appState = await LoadSession();
		if (appState == null)
			return;
		foreach (var tab in appState.Tabs)
		{
			switch (tab.Tab)
			{
				case Tab.Archive:
					if (tab is ArchiveTabState arcTab)
					{
						var archive = await Archives.GetOrAddArchive(arcTab.Id);
						if (archive != null)
							Tabs.OpenTab(Tab.Archive, false, archive, arcTab);
					}

					break;
				case Tab.ArchiveEdit:
					if (tab is IdTabState idTab)
					{
						var archive = await Archives.GetOrAddArchive(idTab.Id);
						if (archive != null)
							Tabs.OpenTab(Tab.ArchiveEdit, false, archive);
					}
					break;
				case Tab.SearchResults:
					if (tab is SearchTabState searchTab)
						Tabs.OpenTab(Tab.SearchResults, false, searchTab.Search);
					break;
			}
		}
	}

	private async Task<AppState?> LoadSession()
	{
		var main = Path.Combine(SessionDir, $"{Settings.Profile.UID}.json");
		var backup = Path.Combine(SessionDir, $"{Settings.Profile.UID}.json.old");
		var appState = await DeserializeSession(main) ?? await DeserializeSession(backup);
		return appState;
	}

	private async Task<AppState?> DeserializeSession(string path)
	{
		if (File.Exists(path))
			try
			{
				var appState = JsonSerializer.Deserialize<AppState>(await Files.GetFile(path), JsonSettings.Options)!;
				return appState;
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
			}
		return null;
	}
}
