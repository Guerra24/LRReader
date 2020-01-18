using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public class ArchivesProvider
	{
		public List<Archive> Archives = new List<Archive>();
		public List<TagStats> TagStats = new List<TagStats>();

		public async Task<bool> LoadArchives()
		{
			Archives.Clear();
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archivelist");

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<List<Archive>>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				await Task.Run(() => Archives.AddRange(result.Data.OrderBy(a => a.title)));
				return true;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

		public async Task<bool> LoadTagStats()
		{
			TagStats.Clear();
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/tagstats");

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<List<TagStats>>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}

			if (result.OK)
			{
				await Task.Run(() =>
				{
					TagStats.AddRange(result.Data.OrderByDescending(a => a.weight));
				});
				return true;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

		public async Task<ArchiveSearch> GetArchivesForPage(int page, string query, bool isnew)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/search");

			rq.AddParameter("start", 100 * page); // TODO: Page Size
			rq.AddParameter("newonly", isnew.ToString().ToLower());
			rq.AddParameter("filter", query);

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

	}
}
