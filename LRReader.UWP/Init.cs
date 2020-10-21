using LRReader.Internal;
using LRReader.Shared.Internal;
using LRReader.UWP.Impl;
using Windows.Storage;

namespace LRReader.UWP
{
	public static class Init
	{
		public static void InitObjects()
		{
			SharedGlobal.SettingsStorage = new SettingsStorage();
			SharedGlobal.FilesStorage = new FilesStorage();
			ArchivesManager.TemporaryFolder = ApplicationData.Current.TemporaryFolder.Path;
			Global.Init(); // Init global static data
		}
	}
}
