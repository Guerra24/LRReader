using LRReader.Shared.Tools;
using LRReader.Shared.ViewModels;
using LRReader.Shared.ViewModels.Base;
using LRReader.Shared.ViewModels.Items;
using LRReader.Shared.ViewModels.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public delegate void ConfigureServices(ServiceCollection collection);

	public class Service
	{
		public static IServiceProvider Services { get; set; }

		private static volatile bool Loaded;

		public static void BuildServices(ConfigureServices? services = null)
		{
			var collection = new ServiceCollection();
			collection.AddLogging();

			// Services
			collection.AddSingleton<ISettingsStorageService, StubSettingsStorageService>();
			collection.AddSingleton<IFilesService, StubFilesService>();
			collection.AddSingleton<IDispatcherService, StubDispatcherService>();
			collection.AddSingleton<PlatformService, StubPlatformService>();
			collection.AddSingleton<UpdatesService, StubUpdatesService>();
			collection.AddSingleton<ArchivesService>();
			collection.AddSingleton<SettingsService>();
			collection.AddSingleton<ImagesService>();
			collection.AddSingleton<EventsService>();
			collection.AddSingleton<ApiService>();
			collection.AddSingleton<TabsService>();

			// Tools
			collection.AddSingleton<DeduplicationTool>();

			// Pages
			collection.AddSingleton<ArchivesPageViewModel>();
			collection.AddSingleton<BookmarksTabViewModel>();
			collection.AddSingleton<CategoriesViewModel>();
			collection.AddSingleton<LoadingPageViewModel>();

			collection.AddTransient<SearchResultsViewModel>();
			collection.AddTransient<ArchiveEditViewModel>();
			collection.AddTransient<ArchivePageViewModel>();
			collection.AddTransient<CategoryEditViewModel>();
			collection.AddTransient<ArchiveItemViewModel>();
			collection.AddTransient<CategoryBaseViewModel>();

			collection.AddTransient<ArchiveHitViewModel>();

			// Tool's Pages
			collection.AddTransient<DeduplicatorToolViewModel>();
			collection.AddTransient<BulkEditorViewModel>();

			services?.Invoke(collection);
			Services = collection.BuildServiceProvider();
		}

		public static async Task InitServices()
		{
			if (Loaded)
				return;
			Loaded = true;
			Dispatcher.Init();
			Platform.Init();
			await SettingsStorage.Init();
			await Settings.Init();
			await Images.Init();
			Logger<Service>().LogInformation("Services initialized");
		}

		public static ISettingsStorageService SettingsStorage => Services.GetRequiredService<ISettingsStorageService>();
		public static IFilesService Files => Services.GetRequiredService<IFilesService>();
		public static IDispatcherService Dispatcher => Services.GetRequiredService<IDispatcherService>();
		public static PlatformService Platform => Services.GetRequiredService<PlatformService>();
		public static UpdatesService Updates => Services.GetRequiredService<UpdatesService>();
		public static ImageProcessingService ImageProcessing => Services.GetRequiredService<ImageProcessingService>();
		public static ArchivesService Archives => Services.GetRequiredService<ArchivesService>();
		public static SettingsService Settings => Services.GetRequiredService<SettingsService>();
		public static ImagesService Images => Services.GetRequiredService<ImagesService>();
		public static EventsService Events => Services.GetRequiredService<EventsService>();
		public static ApiService Api => Services.GetRequiredService<ApiService>();
		public static TabsService Tabs => Services.GetRequiredService<TabsService>();
		public static ILogger<T> Logger<T>() => Services.GetRequiredService<ILogger<T>>();

	}

	public interface IService
	{
		Task Init();
	}
}
