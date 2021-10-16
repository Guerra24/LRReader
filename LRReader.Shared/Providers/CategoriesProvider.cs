using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using Microsoft.Toolkit.Mvvm.Messaging;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers
{
	public static class CategoriesProvider
	{

		public static async Task<bool> Validate()
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories");

			var r = await client.ExecuteAsync(rq, Method.HEAD);

			if (r.StatusCode != HttpStatusCode.OK)
				return false;

			//var decoded = GetCategories();
			//return decoded != null;
			return true;
		}

		public static async Task<List<Category>> GetCategories()
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories");

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<List<Category>>();
		}

		public static async Task<Category> CreateCategory(string name, string search = "", bool pinned = false)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories", Method.PUT);
			rq.AddQueryParameter("name", name);
			rq.AddQueryParameter("search", search);
			rq.AddQueryParameter("pinned", (pinned ? 1 : 0).ToString());

			var r = await client.ExecuteAsync(rq);

			var result = await r.GetResultInternal<CategoryCreatedApiResult>();

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("Network Error", r.ErrorMessage));
				return null;
			}
			if (result.OK)
			{
				return new Category() { id = result.Data.category_id, name = name, search = search, pinned = pinned, archives = new List<string>() };
			}
			else
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification(result.Error.operation, result.Error.error));
				return null;
			}
		}

		public static async Task<bool> UpdateCategory(string id, string name = "", string search = "", bool pinned = false)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories/{id}", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddQueryParameter("name", name);
			rq.AddQueryParameter("search", search);
			rq.AddQueryParameter("pinned", (pinned ? 1 : 0).ToString());

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<bool> DeleteCategory(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories/{id}", Method.DELETE);
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<bool> AddArchiveToCategory(string id, string archive)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories/{id}/{archive}", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddParameter("archive", archive, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<bool> RemoveArchiveFromCategory(string id, string archive)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories/{id}/{archive}", Method.DELETE);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddParameter("archive", archive, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<Category> GetCategory(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/categories/{id}");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<Category>();
		}

	}

	public class CategoryCreatedApiResult : GenericApiResult
	{
		public string category_id { get; set; }
	}
}
