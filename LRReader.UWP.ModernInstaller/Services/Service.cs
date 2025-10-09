using LRReader.UWP.Installer.ViewModels;
using LRReader.UWP.Servicing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LRReader.UWP.Installer.Services;

public static class Service
{
	public static IServiceProvider Services { get; private set; } = null!;

	public static void BuildServices(AppInfo appInfo)
	{
		var collection = new ServiceCollection();

		collection.AddSingleton<InstallerService>();
		collection.AddSingleton(appInfo);
		collection.AddSingleton<CertUtil>();

		collection.AddTransient<InstallerPageViewModel>();

		Services = collection.BuildServiceProvider();
	}

	public static InstallerService Installer => Services.GetRequiredService<InstallerService>();

}
