using LRReader.Shared.Models.Main;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers;

public static class TankoubonsProvider
{

	public static async Task<TankoubonsList?> GetTankoubons(int page = 0)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/tankoubons");
		rq.AddQueryParameter("page", page.ToString());

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<TankoubonsList>().ConfigureAwait(false);
	}

	public static async Task<TankoubonsItem?> GetTankoubon(string id, int page = 0)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/tankoubons/{id}");
		rq.AddUrlSegment("id", id);
		rq.AddQueryParameter("include_full_data", "0");
		rq.AddQueryParameter("page", page.ToString());

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<TankoubonsItem>().ConfigureAwait(false);
	}

	public static async Task<Tankoubon?> CreateTankoubon(string name)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/tankoubons");
		rq.AddQueryParameter("name", name);

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		var res = await r.GetResult<TankoubonCreateApiResult>().ConfigureAwait(false);

		if (res != null)
			return new Tankoubon() { id = res.tankoubon_id, name = name, archives = new List<string>() };
		else
			return null;
	}

	public static async Task<bool> AddArchive(string id, string archive)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/tankoubons/{id}/{archive}");
		rq.AddUrlSegment("id", id);
		rq.AddUrlSegment("archive", archive);

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<bool> RemoveArchive(string id, string archive)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/tankoubons/{id}/{archive}");
		rq.AddUrlSegment("id", id);
		rq.AddUrlSegment("archive", archive);

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<bool> DeleteTankoubon(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/tankoubons/{id}");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}
}
