using LRReader.Shared.Models.Main;
using RestSharp;
using System.Net;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers;

public static class ServerProvider
{

	public static async Task<ServerInfo?> GetServerInfo()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/info");

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		var result = await r.GetResultInternal<ServerInfo>().ConfigureAwait(false);

		if (!string.IsNullOrEmpty(r.ErrorMessage))
		{
			//SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
			return null;
		}
		switch (result.Code)
		{
			case HttpStatusCode.OK:
				return result.Data;
			case HttpStatusCode.Unauthorized:
				return new ServerInfo() { _unauthorized = true };
			default:
				return null;
		}
	}

	public static async Task<List<Plugin>?> GetPlugins(PluginType type)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/plugins/{type}");
		rq.AddUrlSegment("type", type.ToString().ToLower());

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<List<Plugin>>().ConfigureAwait(false);
	}

	public static async Task<UsePluginResult?> UsePlugin(string plugin, string arcid = "", string arg = "")
	{
		var client = Api.Client;

		var rq = new RestRequest("api/plugins/use");
		rq.AddQueryParameter("plugin", plugin);
		rq.AddQueryParameter("id", arcid);
		rq.AddQueryParameter("arg", arg);

		var r = await client.ExecutePostAsync(rq).ConfigureAwait(false);

		return await r.GetResult<UsePluginResult>().ConfigureAwait(false);
	}

	public static async Task<MinionStatus?> GetMinionStatus(int job)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/minion/{job}");
		rq.AddUrlSegment("job", job);

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<MinionStatus>().ConfigureAwait(false);
	}

	public static async Task<RegistriesResult?> GetRegistries()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries");

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<RegistriesResult>().ConfigureAwait(false);
	}

	public static async Task<RegistryResult?> AddRegistry(BaseRegistry registry)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries");
		rq.AddRequestObject(registry);

		var r = await client.ExecutePostAsync(rq).ConfigureAwait(false);

		return await r.GetResult<RegistryResult>().ConfigureAwait(false);
	}

	public static async Task<RegistryMetadataResult?> GetRegistry(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries/{id}");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<RegistryMetadataResult>().ConfigureAwait(false);
	}

	public static async Task<RegistryUpdatedResult?> UpdateRegistry(string id, BaseRegistry registry)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries/{id}");
		rq.AddUrlSegment("id", id);
		rq.AddRequestObject(registry);

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		return await r.GetResult<RegistryUpdatedResult>().ConfigureAwait(false);
	}

	public static async Task<bool> DeleteRegistry(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries/{id}");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<bool> RefreshRegistry(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries/{id}/refresh");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecutePostAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<PluginInstallResult?> InstallPlugin(PluginInstall plugin)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/plugins/install");

		rq.AddRequestObject(plugin);

		var r = await client.ExecutePostAsync(rq).ConfigureAwait(false);

		return await r.GetResult<PluginInstallResult>().ConfigureAwait(false);
	}

	public static async Task<bool> UninstallPlugin(string pluginNamespace)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/plugins/installed/{plugin_namespace}");
		rq.AddUrlSegment("plugin_namespace", pluginNamespace);

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<RegistryDefaultResult?> GetDefaultRegistry()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries/default");

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<RegistryDefaultResult>().ConfigureAwait(false);
	}

	public static async Task<RegistryDefaultResult?> RemoveDefaultRegistry()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries/default");

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult<RegistryDefaultResult>().ConfigureAwait(false);
	}

	public static async Task<RegistryDefaultResult?> GetArchiveCategories(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/registries/default/{id}");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		return await r.GetResult<RegistryDefaultResult>().ConfigureAwait(false);
	}

}
