using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using System.Collections.ObjectModel;

namespace LRReader.Shared.ViewModels.Tools;

public partial class RegistryManagerViewModel : ObservableObject
{

	private readonly IDispatcherService Dispatcher;

	public ObservableCollection<Registry> Registries = new();
	public ObservableCollection<RegistryIndexPlugin> Plugins = new();

	[ObservableProperty]
	private string _registryName = "";
	[ObservableProperty]
	private RegistryProvider _registryProvider;

	[ObservableProperty]
	private string? _registryUrl;
	[ObservableProperty]
	private string? _registryRef;
	[ObservableProperty]
	private string? _registryPath;

	public RegistryManagerViewModel(IDispatcherService dispatcher)
	{
		Dispatcher = dispatcher;
	}

	public async void SelectRegistry(Registry? registry)
	{
		if (registry != null)
		{
			RegistryName = registry.name;
			RegistryProvider = registry.provider;
			RegistryUrl = registry.url;
			RegistryRef = registry.gitRef;
			RegistryPath = registry.path;
			await RefreshRegistry(registry);
		}
		else
		{
			RegistryName = "";
			RegistryProvider = RegistryProvider.Github;
			RegistryUrl = null;
			RegistryRef = null;
			RegistryPath = null;
			Plugins.Clear();
		}
	}

	[RelayCommand]
	public async Task LoadRegistries()
	{
		Registries.Clear();
		var result = await ServerProvider.GetRegistries();
		if (result != null)
		{
			await Task.Run(async () =>
			{
				foreach (var a in result.registries)
					await Dispatcher.RunAsync(() => Registries.Add(a));
			});
		}
	}

	[RelayCommand]
	private async Task AddRegistry()
	{
		if (!Uri.IsWellFormedUriString(RegistryUrl, UriKind.Absolute))
			return;
		var res = await ServerProvider.AddRegistry(new BaseRegistry() { name = RegistryName, provider = RegistryProvider, url = RegistryUrl, gitRef = RegistryRef, path = RegistryPath });
		if (res != null)
		{
			var registry = await ServerProvider.GetRegistry(res.id);
			if (registry != null)
				Registries.Add(registry.registry);
		}
	}

	[RelayCommand]
	private async Task UpdateRegistry(Registry registry)
	{
		if (registry == null)
			return;
		if (!Uri.IsWellFormedUriString(RegistryUrl, UriKind.Absolute))
			return;

		registry.name = RegistryName;
		registry.provider = RegistryProvider;
		registry.url = RegistryUrl;
		registry.gitRef = RegistryRef;
		registry.path = RegistryPath;

		var res = await ServerProvider.UpdateRegistry(registry.id, registry);

	}

	[RelayCommand]
	private async Task RefreshRegistry(Registry registry)
	{
		if (registry == null)
			return;
		var res = await ServerProvider.RefreshRegistry(registry.id);
		Plugins.Clear();
		if (res != null)
			foreach (var plugin in res.index.plugins)
				Plugins.Add(plugin.Value);
	}

	[RelayCommand]
	private async Task DeleteRegistry(Registry registry)
	{
		if (registry == null)
			return;
		var res = await ServerProvider.DeleteRegistry(registry.id);
		if (res)
			Registries.Remove(registry);
	}

	public async Task InstallPlugin(Registry registry, RegistryIndexPlugin plugin)
	{
		var res = await ServerProvider.InstallPlugin(new PluginInstall { @namespace = plugin._namespace, registry = registry.id, version = plugin.versions.First().Key.ToString(), forced = false });
	}

	public async Task UninstallPlugin(RegistryIndexPlugin plugin)
	{
		var res = await ServerProvider.UninstallPlugin(plugin._namespace);
	}

}
