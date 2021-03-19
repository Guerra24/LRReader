using LRReader.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.UWP.ViewModels
{
	public class ViewModelLocator
	{

		public ArchivesPageViewModel ArchivesPageInstance => Service.Services.GetRequiredService<ArchivesPageViewModel>();
		public SettingsPageViewModel SettingsPageInstance => Service.Services.GetRequiredService<SettingsPageViewModel>();
		public BookmarksTabViewModel BookmarksTabInstance => Service.Services.GetRequiredService<BookmarksTabViewModel>();
		public FirstRunPageViewModel FirstRunPageInstance => Service.Services.GetRequiredService<FirstRunPageViewModel>();
		public CategoriesViewModel CategoriesTabInstance => Service.Services.GetRequiredService<CategoriesViewModel>();
		public LoadingPageViewModel LoadingPageInstance => Service.Services.GetRequiredService<LoadingPageViewModel>();
	}
}
