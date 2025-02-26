using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using RestSharp;
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

}
