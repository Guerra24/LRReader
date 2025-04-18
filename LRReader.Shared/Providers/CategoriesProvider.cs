using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers;

public static class CategoriesProvider
{

	public static async Task<bool> Validate()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories");

		var r = await client.ExecuteHeadAsync(rq);

		if (r.StatusCode != HttpStatusCode.OK)
			return false;

		//var decoded = GetCategories();
		//return decoded != null;
		return true;
	}

	public static async Task<List<Category>?> GetCategories()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories");

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<List<Category>>().ConfigureAwait(false);
	}

	public static async Task<Category?> CreateCategory(string name, string search = "", bool pinned = false)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories");
		rq.AddQueryParameter("name", name);
		rq.AddQueryParameter("search", search);
		rq.AddQueryParameter("pinned", (pinned ? 1 : 0).ToString());

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		var result = await r.GetResultInternal<CategoryId>().ConfigureAwait(false);

		if (!string.IsNullOrEmpty(r.ErrorMessage))
		{
			WeakReferenceMessenger.Default.Send(new ShowNotification("Network Error", r.ErrorMessage, severity: NotificationSeverity.Error));
			return null;
		}
		if (result.OK)
		{
			return new Category() { id = result.Data!.category_id, name = name, search = search, pinned = pinned, archives = new List<string>() };
		}
		else
		{
			WeakReferenceMessenger.Default.Send(new ShowNotification(result?.Error?.operation ?? "", result?.Error?.error, severity: NotificationSeverity.Error));
			return null;
		}
	}

	public static async Task<bool> UpdateCategory(string id, string name = "", string search = "", bool pinned = false)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/{id}");
		rq.AddUrlSegment("id", id);
		rq.AddQueryParameter("name", name);
		rq.AddQueryParameter("search", search);
		rq.AddQueryParameter("pinned", (pinned ? 1 : 0).ToString());

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<bool> DeleteCategory(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/{id}");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<bool> AddArchiveToCategory(string id, string archive)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/{id}/{archive}");
		rq.AddUrlSegment("id", id);
		rq.AddUrlSegment("archive", archive);

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<bool> RemoveArchiveFromCategory(string id, string archive)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/{id}/{archive}");
		rq.AddUrlSegment("id", id);
		rq.AddUrlSegment("archive", archive);

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult().ConfigureAwait(false);
	}

	public static async Task<Category?> GetCategory(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/{id}");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<Category>().ConfigureAwait(false);
	}

	public static async Task<CategoryId?> GetBookmarkLink()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/bookmark_link");

		var r = await client.ExecuteGetAsync(rq).ConfigureAwait(false);

		return await r.GetResult<CategoryId>().ConfigureAwait(false);
	}

	public static async Task<CategoryId?> SetBookmarkLink(string id)
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/bookmark_link/{id}");
		rq.AddUrlSegment("id", id);

		var r = await client.ExecutePutAsync(rq).ConfigureAwait(false);

		return await r.GetResult<CategoryId>().ConfigureAwait(false);
	}

	public static async Task<CategoryId?> DeleteBookmarkLink()
	{
		var client = Api.Client;

		var rq = new RestRequest("api/categories/bookmark_link");

		var r = await client.ExecuteDeleteAsync(rq).ConfigureAwait(false);

		return await r.GetResult<CategoryId>().ConfigureAwait(false);
	}
}
