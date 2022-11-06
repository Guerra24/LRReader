using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using RestSharp;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers
{
	public class ShinobuProvider
	{

		public static async Task<ShinobuStatus?> GetShinobuStatus()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/shinobu");

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResultNoError<ShinobuStatus>();
		}

		public static async Task<bool> RestartWorker()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/shinobu/restart");

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult();
		}

		public static async Task<bool> StopWorker()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/shinobu/stop");

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult();
		}

		public static async Task<ShinobuRescan?> Rescan()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/shinobu/rescan");

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResultNoError<ShinobuRescan>();
		}

	}

}
