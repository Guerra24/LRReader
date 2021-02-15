using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using System;

namespace LRReader.Shared.Internal
{
	public class SharedGlobal
	{
		public static LRRApi LRRApi { get; set; }
		public static SharedEventManager EventManager { get; set; }
		public static ISettingsStorage SettingsStorage { get; set; } = new StubSettingsStorage();
		public static SettingsManager SettingsManager { get; set; }
		public static ArchivesManager ArchivesManager { get; set; }
		public static UpdatesManager UpdatesManager { get; } = new UpdatesManager();
		public static ServerInfo ServerInfo { get; set; }
		public static ImagesManager ImagesManager { get; set; } = new ImagesManager();
		public static ControlFlags ControlFlags { get; set; } = new ControlFlags();
	}

	public class ControlFlags
	{
		public bool CategoriesEnabled = true;
		public bool CategoriesV2 = false;
		public bool ServerSideProgress = false;

		public void Check(ServerInfo serverInfo)
		{
			if (serverInfo.version == new Version(0, 7, 5))
				CategoriesEnabled = false;
			if (serverInfo.version >= new Version(0, 7, 7))
			{
				CategoriesV2 = true;
				ServerSideProgress = true;
			}
		}
	}
}
