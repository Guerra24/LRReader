﻿using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Shared.ViewModels
{
	public class ViewModelLocator
	{
		public ArchivesPageViewModel ArchivesPageInstance => Service.Services.GetRequiredService<ArchivesPageViewModel>();
		public SettingsPageViewModel SettingsPageInstance => Service.Services.GetRequiredService<SettingsPageViewModel>();
		public BookmarksTabViewModel BookmarksTabInstance => Service.Services.GetRequiredService<BookmarksTabViewModel>();
		public CategoriesViewModel CategoriesTabInstance => Service.Services.GetRequiredService<CategoriesViewModel>();
		public LoadingPageViewModel LoadingPageInstance => Service.Services.GetRequiredService<LoadingPageViewModel>();
		public SearchResultsViewModel SearchResultsInstance => Service.Services.GetRequiredService<SearchResultsViewModel>();
		public ArchiveEditViewModel ArchiveEditInstance => Service.Services.GetRequiredService<ArchiveEditViewModel>();
		public ArchivePageViewModel ArchivePageInstance => Service.Services.GetRequiredService<ArchivePageViewModel>();
		public CategoryEditViewModel CategoryEditInstance => Service.Services.GetRequiredService<CategoryEditViewModel>();
		public TabsService HostTabPageInstance => Service.Services.GetRequiredService<TabsService>();
		public DeduplicatorToolViewModel DeduplicatorToolViewModelInstance => Service.Services.GetRequiredService<DeduplicatorToolViewModel>();
		public DeduplicatorHiddenViewModel DeduplicatorHiddenViewModelInstance => Service.Services.GetRequiredService<DeduplicatorHiddenViewModel>();
		public BulkEditorViewModel BulkEditorViewModelInstance => Service.Services.GetRequiredService<BulkEditorViewModel>();
		public TankoubonsViewModel TankoubonsViewModelInstance => Service.Services.GetRequiredService<TankoubonsViewModel>();
		public TankoubonViewModel TankoubonViewModelInstance => Service.Services.GetRequiredService<TankoubonViewModel>();
		public TankoubonEditViewModel TankoubonEditViewModelInstance => Service.Services.GetRequiredService<TankoubonEditViewModel>();
	}
}
