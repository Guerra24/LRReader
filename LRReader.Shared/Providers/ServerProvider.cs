using LRReader.Shared.Models.Main;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers
{
	public static class ServerProvider
	{

		public static async Task<ServerInfo?> GetServerInfo()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/info");

			var r = await client.ExecuteGetAsync(rq);

			var result = await r.GetResultInternal<ServerInfo>();

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
			rq.AddParameter("type", type.ToString().ToLower(), ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<List<Plugin>>();
		}

		public static async Task<UsePluginResult?> UsePlugin(string plugin, string arcid = "", string arg = "")
		{
			var client = Api.Client;

			var rq = new RestRequest("api/plugins/use");
			rq.AddQueryParameter("plugin", plugin);
			rq.AddQueryParameter("id", arcid);
			rq.AddQueryParameter("arg", arg);

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult<UsePluginResult>();
		}

		public static async Task<MinionStatus?> GetMinionStatus(int job)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/minion/{job}");
			rq.AddParameter("job", job, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<MinionStatus>();
		}
	}
}
