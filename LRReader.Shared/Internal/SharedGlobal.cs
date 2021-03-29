using LRReader.Shared.Models.Main;
using System;

namespace LRReader.Shared.Internal
{
	public class SharedGlobal
	{
		public static ApiConnection ApiConnection { get; set; }
		public static SharedEventManager EventManager { get; set; }
		public static ArchivesManager ArchivesManager { get; set; }
		public static UpdatesManager UpdatesManager { get; } = new UpdatesManager();
		public static ServerInfo ServerInfo { get; set; }
		public static ControlFlags ControlFlags { get; set; } = new ControlFlags();
	}

	public class ControlFlags
	{
		public bool CategoriesEnabled = true;
		public bool V077 = false;

		public void Check(ServerInfo serverInfo)
		{
			if (serverInfo.version == new Version(0, 7, 5))
				CategoriesEnabled = false;
			V077 = serverInfo.version >= new Version(0, 7, 7);
		}
	}
}
