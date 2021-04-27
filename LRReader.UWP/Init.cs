using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Services;
using LRReader.UWP.ViewModels;
using LRReader.UWP.ViewModels.Items;
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

				collection.AddSingleton<ArchivesPageViewModel>();
				collection.AddSingleton<SettingsPageViewModel>();
				collection.AddSingleton<BookmarksTabViewModel>();
				collection.AddSingleton<FirstRunPageViewModel>();
				collection.AddSingleton<CategoriesViewModel>();
				collection.AddSingleton<LoadingPageViewModel>();

				collection.AddTransient<SearchResultsViewModel>();
				collection.AddTransient<ArchiveEditViewModel>();
				collection.AddTransient<ArchivePageViewModel>();
				collection.AddTransient<CategoryEditViewModel>();
				collection.AddTransient<ArchiveItemViewModel>();
			});
		}
	}
}
