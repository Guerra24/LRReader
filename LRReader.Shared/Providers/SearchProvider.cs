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
	public static class SearchProvider
	{
		public static async Task<ArchiveSearch> Search(int archivesPerPage, int page, string query, string category, bool isnew, bool untagged)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/search");

			rq.AddParameter("start", archivesPerPage * page);
			rq.AddParameter("newonly", isnew.ToString().ToLower());
			rq.AddParameter("untaggedonly", untagged.ToString().ToLower());
			rq.AddParameter("filter", query);
			rq.AddParameter("category", category);
			rq.AddParameter("order", "asc");

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<ArchiveSearch>(r);

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

		public static async Task<bool> DiscardCache()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/search/cache", Method.DELETE);

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
