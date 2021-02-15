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
			FilesStorage.SetProvider(new UWPFilesStorage());
			ArchivesManager.TemporaryFolder = ApplicationData.Current.LocalCacheFolder.Path;
			Global.Init(); // Init global static data
		}
	}
}
