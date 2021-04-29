using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.ViewModels
{
	public class ViewModelLocator
	{
		public ArchivesPageViewModel ArchivesPageInstance => Service.Services.GetRequiredService<ArchivesPageViewModel>();
		//public SettingsPageViewModel SettingsPageInstance => Service.Services.GetRequiredService<SettingsPageViewModel>();
		public BookmarksTabViewModel BookmarksTabInstance => Service.Services.GetRequiredService<BookmarksTabViewModel>();
		//public FirstRunPageViewModel FirstRunPageInstance => Service.Services.GetRequiredService<FirstRunPageViewModel>();
		public CategoriesViewModel CategoriesTabInstance => Service.Services.GetRequiredService<CategoriesViewModel>();
		public LoadingPageViewModel LoadingPageInstance => Service.Services.GetRequiredService<LoadingPageViewModel>();
		public SearchResultsViewModel SearchResultsInstance => Service.Services.GetRequiredService<SearchResultsViewModel>();
		public ArchiveEditViewModel ArchiveEditInstance => Service.Services.GetRequiredService<ArchiveEditViewModel>();
		public ArchivePageViewModel ArchivePageInstance => Service.Services.GetRequiredService<ArchivePageViewModel>();
		public CategoryEditViewModel CategoryEditInstance => Service.Services.GetRequiredService<CategoryEditViewModel>();
		public TabsService HostTabPageInstance => Service.Services.GetRequiredService<TabsService>();
	}
}
