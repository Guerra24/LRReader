using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using System;

namespace LRReader.Shared.Internal
{
	public class SharedGlobal
	{
		public static ApiConnection ApiConnection { get; set; }
		public static UpdatesManager UpdatesManager { get; } = new UpdatesManager();
		public static ServerInfo ServerInfo { get; set; }
		public static ControlFlags ControlFlags { get; set; } = new ControlFlags();
	}

	public class ControlFlags
	{
		public bool CategoriesEnabled = true;
		public bool V077 = false;
		public bool V078 = false;

		public bool V078Edit => V078 & Service.Settings.Profile.HasApiKey;
		public bool CategoriesEnabledEdit => CategoriesEnabled & Service.Settings.Profile.HasApiKey;

		public void Check(ServerInfo serverInfo)
		{
			if (serverInfo.version == new Version(0, 7, 5))
				CategoriesEnabled = false;
			V077 = serverInfo.version >= new Version(0, 7, 7);
			V078 = serverInfo.version >= new Version(0, 7, 8);
		}
	}
}
