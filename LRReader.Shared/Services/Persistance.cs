﻿using LRReader.Shared.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Sentry;

namespace LRReader.Shared.Services
{
	public class Persistance
	{
		private readonly IFilesService Files;
		private readonly TabsService Tabs;
		private readonly SettingsService Settings;
		private readonly ArchivesService Archives;

		public Persistance(IFilesService files, TabsService tabs, SettingsService settings, ArchivesService archives)
		{
			Files = files;
			Tabs = tabs;
			Settings = settings;
			Archives = archives;
		}

		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]
		public async Task Suspend()
		{
			if (Settings.Profile != null)
			{
				var appState = new AppState();
				appState.ProfileUID = Settings.Profile.UID;
				foreach (var tab in Tabs.TabItems)
					appState.Tabs.Add(tab.GetTabState());

				await Files.StoreFileSafe(Path.Combine(Files.Local, "Suspended.json"), JsonSerializer.Serialize(appState, JsonSettings.Options));
			}
		}
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026")]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050")]

		public async Task Restore()
		{
			var path = Path.Combine(Files.Local, "Suspended.json");
			if (!File.Exists(path))
				return;
			try
			{
				var appState = JsonSerializer.Deserialize<AppState>(await Files.GetFile(path), JsonSettings.Options)!;
				if (Settings.Profile.UID != appState.ProfileUID)
					return;
				foreach (var tab in appState.Tabs)
				{
					switch (tab.Tab)
					{
						case Tab.Archive:
							if (tab is ArchiveTabState arcTab && Archives.TryGetArchive(arcTab.Id, out var archive))
								Tabs.OpenTab(Tab.Archive, false, archive, arcTab.Page, arcTab.WasOpen);
							break;
						case Tab.ArchiveEdit:
							if (tab is IdTabState idTab && Archives.TryGetArchive(idTab.Id, out archive))
								Tabs.OpenTab(Tab.ArchiveEdit, false, archive);
							break;
					}
				}
				File.Delete(path);
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
			}
		}
	}
}
