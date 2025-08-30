using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers;

public static class DatabaseProvider
{

	public static async Task<bool> Validate()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/database/stats");

		var r = await client.ExecuteHeadAsync(rq);

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

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<List<TagStats>>().ConfigureAwait(false);
	}

	public static async Task<DatabaseCleanResult?> CleanDatabase()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/database/clean");

		var r = await client.ExecutePostAsync(rq).ConfigureAwait(false);

		return await r.GetResult<DatabaseCleanResult>().ConfigureAwait(false);
	}

	public static async Task<bool> DropDatabase()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/database/drop");

		var r = await client.ExecutePostAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<DownloadPayload?> BackupJSON()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/database/backup");

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		if (!string.IsNullOrEmpty(r.ErrorMessage))
		{
			WeakReferenceMessenger.Default.Send(new ShowNotification("Network Error", r.ErrorMessage, severity: NotificationSeverity.Error));
			return null;
		}
		switch (r.StatusCode)
		{
			case HttpStatusCode.OK:
				var download = new DownloadPayload
				{
					Data = r.RawBytes!,
					Name = "database_backup.json",
					Type = ".json"
				};
				return download;
			default:
				var error = await r.GetError().ConfigureAwait(false);
				WeakReferenceMessenger.Default.Send(new ShowNotification(error.operation, error.error, severity: NotificationSeverity.Error));
				return null;
		}
	}

	public static async Task<bool> ClearAllNew()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/database/isnew", Method.Delete);

		var r = await client.ExecuteAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

}
