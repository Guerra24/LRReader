using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using System.Collections.ObjectModel;

namespace LRReader.Shared.ViewModels;

public partial class RegistryManagerViewModel : ObservableObject
{

	private readonly IDispatcherService Dispatcher;

	public ObservableCollection<Registry> Registries = new();

	[ObservableProperty]
	private string _registryName = "";
	[ObservableProperty]
	private RegistryType _registryType;

	partial void OnRegistryTypeChanged(RegistryType value)
	{
		if (value == RegistryType.Local)
			RegistryProvider = null;
	}

	[ObservableProperty]
	private RegistryProvider? _registryProvider;
	[ObservableProperty]
	private string? _registryUrl;
	[ObservableProperty]
	private string? _registryRef;
	[ObservableProperty]
	private string? _registryPath;

	[ObservableProperty]
	private string _pluginNamespace = "";
	[ObservableProperty]
	private string? _pluginVersion;
	[ObservableProperty]
	private bool _pluginForced;


	public RegistryManagerViewModel(IDispatcherService dispatcher)
	{
		Dispatcher = dispatcher;
	}

	public void SelectRegistry(Registry? registry)
	{
		if (registry != null)
		{
			RegistryName = registry.name;
			RegistryType = registry.type;
			RegistryProvider = registry.provider;
			RegistryUrl = registry.url;
			RegistryRef = registry.gitRef;
			RegistryPath = registry.path;
		}
		else
		{
			RegistryName = "";
			RegistryType = RegistryType.Git;
			RegistryProvider = null;
			RegistryUrl = null;
			RegistryRef = null;
			RegistryPath = null;
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
		var res = await ServerProvider.AddRegistry(new BaseRegistry() { name = RegistryName, type = RegistryType, provider = RegistryProvider, url = RegistryUrl, gitRef = RegistryRef, path = RegistryPath });
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
		registry.type = RegistryType;
		registry.provider = RegistryProvider;
		registry.url = RegistryUrl;
		registry.gitRef = RegistryRef;
		registry.path = RegistryPath;

		var res = await ServerProvider.UpdateRegistry(registry.id, registry);

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

	[RelayCommand]
	private async Task RefreshRegistry(Registry registry)
	{
		if (registry == null)
			return;
		var res = await ServerProvider.RefreshRegistry(registry.id);
	}

	[RelayCommand]
	private async Task InstallPlugin(Registry registry)
	{
		if (registry == null)
			return;
		var res = await ServerProvider.InstallPlugin(new PluginInstall { @namespace = PluginNamespace, registry = registry.id, version = PluginVersion, forced = PluginForced });
	}

}
