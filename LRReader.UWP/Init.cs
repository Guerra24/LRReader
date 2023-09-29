﻿using LRReader.Shared.Services;
using LRReader.UWP.Services;
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
				collection.Replace(ServiceDescriptor.Singleton<PlatformService, UWPlatformService>());
				collection.Replace(ServiceDescriptor.Singleton<IKarenService, KarenService>());
#if !DEBUG
#if SIDELOAD
				collection.Replace(ServiceDescriptor.Singleton<UpdatesService, SideloadUpdatesService>());
#elif NIGHTLY
				collection.Replace(ServiceDescriptor.Singleton<UpdatesService, NightlyUpdatesService>());
#else
				collection.Replace(ServiceDescriptor.Singleton<UpdatesService, StoreUpdatesService>());
#endif
#endif
				collection.AddSingleton<ImageProcessingService, UWPImageProcessingService>();
			});
		}

	}
}
