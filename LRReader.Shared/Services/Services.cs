using Microsoft.Extensions.DependencyInjection;
using System;

namespace LRReader.Shared.Services
{
	public delegate void ConfigureServices(ServiceCollection collection);

	public class Service
	{
		public static IServiceProvider Services { get; set; }

		public static void Init(ConfigureServices services = null)
		{
			var collection = new ServiceCollection();
			collection.AddSingleton<ISettingsStorageService, StubSettingsStorageService>();
			collection.AddSingleton<IFilesService, StubFilesService>();
			collection.AddSingleton<SettingsService>();
			services?.Invoke(collection);
			Services = collection.BuildServiceProvider();
		}

		public static ISettingsStorageService SettingsStorage => Services.GetRequiredService<ISettingsStorageService>();
		public static IFilesService Files => Services.GetRequiredService<IFilesService>();
		public static SettingsService Settings => Services.GetRequiredService<SettingsService>();

	}
}
