using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;

namespace LRReader.Shared.Providers
{
	public static class ServerProvider
	{

		public static async Task<ServerInfo> GetServerInfo()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/info");

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<ServerInfo>(r);

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
	}
}
