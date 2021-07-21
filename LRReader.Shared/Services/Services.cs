using LRReader.Shared.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public delegate void ConfigureServices(ServiceCollection collection);

	public class Service
	{
		public static IServiceProvider Services { get; set; }

		private static bool Loaded;

		public static void BuildServices(ConfigureServices services = null)
		{
			var collection = new ServiceCollection();
			collection.AddSingleton<ISettingsStorageService, StubSettingsStorageService>();
			collection.AddSingleton<IFilesService, StubFilesService>();
			collection.AddSingleton<IDispatcherService, StubDispatcherService>();
			collection.AddSingleton<IPlatformService, StubPlatformService>();
			collection.AddSingleton<ArchivesService>();
			collection.AddSingleton<SettingsService>();
			collection.AddSingleton<ImagesService>();
			collection.AddSingleton<EventsService>();
			collection.AddSingleton<ApiService>();
			collection.AddSingleton<TabsService>();

			collection.AddSingleton<DeduplicationTool>();

			services?.Invoke(collection);
			Services = collection.BuildServiceProvider();
		}

		public static async Task InitServices()
		{
			if (Loaded)
				return;
			Dispatcher.Init();
			Platform.Init();
			await SettingsStorage.Init();
			await Settings.Init();
			await Images.Init();
			Loaded = true;
		}

		public static ISettingsStorageService SettingsStorage => Services.GetRequiredService<ISettingsStorageService>();
		public static IFilesService Files => Services.GetRequiredService<IFilesService>();
		public static IDispatcherService Dispatcher => Services.GetRequiredService<IDispatcherService>();
		public static IPlatformService Platform => Services.GetRequiredService<IPlatformService>();
		public static ImageProcessingService ImageProcessing => Services.GetRequiredService<ImageProcessingService>();
		public static ArchivesService Archives => Services.GetRequiredService<ArchivesService>();
		public static SettingsService Settings => Services.GetRequiredService<SettingsService>();
		public static ImagesService Images => Services.GetRequiredService<ImagesService>();
		public static EventsService Events => Services.GetRequiredService<EventsService>();
		public static ApiService Api => Services.GetRequiredService<ApiService>();
		public static TabsService Tabs => Services.GetRequiredService<TabsService>();

	}

	public interface IService
	{
		Task Init();
	}
}
