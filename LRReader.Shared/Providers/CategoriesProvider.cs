using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public static class CategoriesProvider
	{

		public static async Task<List<Category>> GetCategories()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/categories");

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<List<Category>>(r);

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

		public static async Task<Category> CreateCategory(string name, string search = "", bool pinned = false)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/categories", Method.PUT);
			rq.AddQueryParameter("name", name);
			rq.AddQueryParameter("search", search);
			rq.AddQueryParameter("pinned", pinned.ToString().ToLower());

			var r = await client.ExecuteAsync(rq);

			var result = await LRRApi.GetResult<CategoryCreatedApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			if (result.OK)
			{
				return new Category() { id = result.Data.category_id, name = name, search = search, pinned = pinned, archives = new List<string>() };
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return null;
			}
		}

		public static async Task<bool> UpdateCategory(string id, string name = "", string search = "", bool pinned = false)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/categories/{id}", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddQueryParameter("name", name);
			rq.AddQueryParameter("search", search);
			rq.AddQueryParameter("pinned", (pinned ? 1 : 0).ToString());

			var r = await client.ExecuteAsync(rq);

			var result = await LRRApi.GetResult<GenericApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return true;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

		public static async Task<bool> DeleteCategory(string id)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/categories/{id}", Method.DELETE);
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			var result = await LRRApi.GetResult<GenericApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return true;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

		public static async Task<bool> AddArchiveToCategory(string id, string archive)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/categories/{id}/{archive}", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddParameter("archive", archive, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			var result = await LRRApi.GetResult<GenericApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return true;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

		public static async Task<bool> RemoveArchiveFromCategory(string id, string archive)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/categories/{id}/{archive}", Method.DELETE);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddParameter("archive", archive, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			var result = await LRRApi.GetResult<GenericApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return true;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

	}

	public class CategoryCreatedApiResult : GenericApiResult
	{
		public string category_id { get; set; }
	}
}
