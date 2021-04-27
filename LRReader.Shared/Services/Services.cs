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
			collection.AddSingleton<ArchivesService>();
			collection.AddSingleton<SettingsService>();
			collection.AddSingleton<ImagesService>();
			collection.AddSingleton<EventsService>();
			services?.Invoke(collection);
			Services = collection.BuildServiceProvider();
		}

		public static async Task InitServices()
		{
			if (Loaded)
				return;
			Dispatcher.Init();
			await Settings.Init();
			await Images.Init();
			Loaded = true;
		}

		public static ISettingsStorageService SettingsStorage => Services.GetRequiredService<ISettingsStorageService>();
		public static IFilesService Files => Services.GetRequiredService<IFilesService>();
		public static IDispatcherService Dispatcher => Services.GetRequiredService<IDispatcherService>();
		public static ArchivesService Archives => Services.GetRequiredService<ArchivesService>();
		public static SettingsService Settings => Services.GetRequiredService<SettingsService>();
		public static ImagesService Images => Services.GetRequiredService<ImagesService>();
		public static EventsService Events => Services.GetRequiredService<EventsService>();

	}

	public interface IService
	{
		Task Init();
	}
}
