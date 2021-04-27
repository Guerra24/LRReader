using LRReader.Shared.Models.Main;
using RestSharp;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers
{
	public static class SearchProvider
	{
		public static async Task<ArchiveSearch> Search(int archivesPerPage, int page, string query, string category, bool isnew, bool untagged, string sortby = "title", Order order = Order.Ascending)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/search");

			rq.AddParameter("start", archivesPerPage * page);
			rq.AddParameter("newonly", isnew.ToString().ToLower());
			rq.AddParameter("untaggedonly", untagged.ToString().ToLower());
			rq.AddParameter("filter", query);
			rq.AddParameter("category", category);
			rq.AddParameter("sortby", sortby);
			rq.AddParameter("order", order.String());

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<ArchiveSearch>();
		}

		public static async Task<bool> DiscardCache()
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/search/cache", Method.DELETE);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}
	}

	public enum Order
	{
		Ascending, Descending
	}

	public static class OrderExtensions
	{
		public static string String(this Order order)
		{
			switch (order)
			{
				case Order.Ascending:
					return "asc";
				case Order.Descending:
					return "desc";
				default:
					return "";
			}
		}
	}
}
