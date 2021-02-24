using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public static class ServerProvider
	{

		public static async Task<ServerInfo> GetServerInfo()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/info");

			var r = await client.ExecuteGetAsync(rq);

			var result = await ApiConnection.GetResult<ServerInfo>(r);

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

		public static async Task<List<Plugin>> GetPlugins(PluginType type)
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/plugins/{type}");
			rq.AddParameter("type", type.ToString().ToLower(), ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			var result = await ApiConnection.GetResult<List<Plugin>>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			if (result.OK)
			{
				return result.Data;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return null;
			}
		}

		public static async Task<UsePluginResult> UsePlugin(string plugin, string arcid = "", string arg = "")
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/plugins/use");
			rq.AddQueryParameter("plugin", plugin);
			rq.AddQueryParameter("id", arcid);
			rq.AddQueryParameter("arg", arg);

			var r = await client.ExecutePostAsync(rq);

			var result = await ApiConnection.GetResult<UsePluginResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			if (result.OK)
			{
				return result.Data;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return null;
			}
		}

		public static async Task<MinionStatus> GetMinionStatus(int job)
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/minion/{job}");
			rq.AddParameter("job", job, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			var result = await ApiConnection.GetResult<MinionStatus>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				return null;
			}
			if (result.OK)
			{
				return result.Data;
			}
			else
			{
				return null;
			}
		}
	}
}
