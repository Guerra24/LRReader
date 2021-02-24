using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using RestSharp;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public class ShinobuProvider
	{

		public static async Task<ShinobuStatus> GetShinobuStatus()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/shinobu");

			var r = await client.ExecuteGetAsync(rq);

			var result = await ApiConnection.GetResult<ShinobuStatus>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
				return null;
			if (result.OK)
				return result.Data;
			return null;
		}

		public static async Task<bool> RestartWorker()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/shinobu/restart");

			var r = await client.ExecutePostAsync(rq);

			var result = await ApiConnection.GetResult<GenericApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return result.Data.success;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

		public static async Task<bool> StopWorker()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/shinobu/stop");

			var r = await client.ExecutePostAsync(rq);

			var result = await ApiConnection.GetResult<GenericApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return result.Data.success;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

	}

}
