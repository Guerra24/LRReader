using LRReader.Avalonia.Services;
using LRReader.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LRReader.Avalonia
{
	public static class Init
	{
		public static void EarlyInit()
		{
			Service.BuildServices((ServiceCollection collection) =>
			{
				collection.Replace(ServiceDescriptor.Singleton<ISettingsStorageService, SettingsStorageService>());
				collection.Replace(ServiceDescriptor.Singleton<IFilesService, FilesService>());
				collection.Replace(ServiceDescriptor.Singleton<IDispatcherService, DispatcherService>());
				collection.Replace(ServiceDescriptor.Singleton<PlatformService, AvaloniaPlatformService>());
#if !DEBUG
#if NIGHTLY_APPIMAGE
				collection.Replace(ServiceDescriptor.Singleton<UpdatesService, NightlyAppImageUpdatesService>());
#endif
#endif

				collection.AddSingleton<ImageProcessingService, AvaloniaImageProcessingService>();
			});
		}
	}
}
