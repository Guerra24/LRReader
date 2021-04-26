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
			});
		}
	}
}
