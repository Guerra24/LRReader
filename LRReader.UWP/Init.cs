using LRReader.Shared.Services;
using LRReader.UWP.Services;
using LRReader.UWP.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LRReader.UWP
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
				collection.Replace(ServiceDescriptor.Singleton<IPlatformService, UWPlatformService>());

				collection.AddSingleton<ImageProcessingService, UWPImageProcessingService>();

				collection.AddSingleton<SettingsPageViewModel>();
				collection.AddSingleton<FirstRunPageViewModel>();
			});
		}
	}
}
