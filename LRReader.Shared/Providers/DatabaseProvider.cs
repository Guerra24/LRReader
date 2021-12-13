using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using Microsoft.Toolkit.Mvvm.Messaging;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers
{
	public static class DatabaseProvider
	{

		public static async Task<bool> Validate()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/database/stats");

			var r = await client.ExecuteAsync(rq, Method.HEAD);

			if (r.StatusCode != HttpStatusCode.OK)
				return false;

			//var decoded = GetTagStats();
			//return decoded != null;
			return true;
		}

		public static async Task<List<TagStats>?> GetTagStats()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/database/stats");

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<List<TagStats>>();
		}

		public static async Task<DatabaseCleanResult?> CleanDatabase()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/database/clean");

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult<DatabaseCleanResult>();
		}

		public static async Task<bool> DropDatabase()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/database/drop");

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult();
		}

		public static async Task<DownloadPayload?> BackupJSON()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/database/backup");

			var r = await client.ExecuteGetAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("Network Error", r.ErrorMessage));
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
					WeakReferenceMessenger.Default.Send(new ShowNotification(error.operation, error.error));
					return null;
			}
		}

		public static async Task<bool> ClearAllNew()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/database/isnew", Method.DELETE);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

	}
}
