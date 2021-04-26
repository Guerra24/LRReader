using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public static class DatabaseProvider
	{

		public static async Task<List<TagStats>> GetTagStats()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/database/stats");

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<List<TagStats>>();
		}

		public static async Task<DatabaseCleanResult> CleanDatabase()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/database/clean");

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult<DatabaseCleanResult>();
		}

		public static async Task<bool> DropDatabase()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/database/drop");

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult();
		}

		public static async Task<DownloadPayload> BackupJSON()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/database/backup");

			var r = await client.ExecuteGetAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				Service.Events.ShowNotification("Network Error", r.ErrorMessage);
				return null;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					var download = new DownloadPayload();
					download.Data = r.RawBytes;
					download.Name = "database_backup.json";
					download.Type = ".json";
					return download;
				default:
					var error = await r.GetError();
					Service.Events.ShowNotification(error.title, error.error);
					return null;
			}
		}

		public static async Task<bool> ClearAllNew()
		{
			var client = SharedGlobal.ApiConnection.GetClient();

			var rq = new RestRequest("api/database/isnew", Method.DELETE);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

	}
}
