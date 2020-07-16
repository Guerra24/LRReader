using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public static class DatabaseProvider
	{

		public static async Task<List<TagStats>> GetTagStats()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/database/stats");

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<List<TagStats>>(r);

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

		public static async Task<DatabaseCleanResult> CleanDatabase()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/database/clean");

			var r = await client.ExecutePostAsync(rq);

			var result = await LRRApi.GetResult<DatabaseCleanResult>(r);

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

		public static async Task<bool> DropDatabase()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/database/drop");

			var r = await client.ExecutePostAsync(rq);

			var result = await LRRApi.GetResult<GenericApiResult>(r);

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

		public static async Task<DownloadPayload> BackupJSON()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/database/backup");

			var r = await client.ExecuteGetAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
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
					var error = await LRRApi.GetError(r);
					SharedGlobal.EventManager.ShowError(error.title, error.error);
					return null;
			}
		}

		public static async Task<bool> ClearAllNew()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/database/isnew", Method.DELETE);

			var r = await client.ExecuteAsync(rq);

			var result = await LRRApi.GetResult<GenericApiResult>(r);

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
