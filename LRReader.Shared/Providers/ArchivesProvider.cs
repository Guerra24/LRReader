using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Providers
{
	public static class ArchivesProvider
	{

		public static async Task<List<Archive>> GetArchives()
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives");

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<List<Archive>>();
		}

		public static async Task<Archive> GetArchive(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/metadata");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<Archive>();
		}

		public static async Task<ArchiveCategories> GetArchiveCategories(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/categories");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<ArchiveCategories>();
		}

		public static async Task<byte[]> GetThumbnail(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/thumbnail");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					return r.RawBytes;
				default:
					return null;
			}
		}

		public static async Task<ArchiveImages> ExtractArchive(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/extract");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult<ArchiveImages>();
		}

		public static async Task<DownloadPayload> DownloadArchive(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/download");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				Events.ShowNotification("Network Error", r.ErrorMessage);
				return null;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					var download = new DownloadPayload();
					var header = r.Headers.First(h => h.Name.Equals("Content-Disposition")).Value as string;
					var parms = header.Split(';').Select(s => s.Trim());
					var natr = parms.First(s => s.StartsWith("filename"));
					var nameAndType = natr.Substring(natr.IndexOf("\"") + 1, natr.Length - natr.IndexOf("\"") - 2);

					download.Data = r.RawBytes;
					download.Name = nameAndType.Substring(0, nameAndType.LastIndexOf("."));
					download.Type = nameAndType.Substring(nameAndType.LastIndexOf("."));
					return download;
				default:
					var error = await r.GetError();
					Events.ShowNotification(error.operation, error.error);
					return null;
			}
		}

		public static async Task<bool> ClearNewArchive(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/isnew", Method.DELETE);
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<bool> UpdateArchive(string id, string title = "", string tags = "")
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/metadata", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddQueryParameter("title", title);
			rq.AddQueryParameter("tags", tags);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<DeleteArchiveResult> DeleteArchive(string id)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}", Method.DELETE);
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult<DeleteArchiveResult>();
		}

		public static async Task<byte[]> GetImage(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			var client = Api.GetClient();

			var rq = new RestRequest(path);

			var r = await client.ExecuteGetAsync(rq);

			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					return r.RawBytes;
				default:
					return null;
			}
		}

		public static async Task<bool> UpdateProgress(string id, int progress)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/archives/{id}/progress/{progress}", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddParameter("progress", progress, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<MinionJob> RegenerateThumbnails(bool force = false)
		{
			var client = Api.GetClient();

			var rq = new RestRequest("api/regen_thumbs");
			rq.AddQueryParameter("force", (force ? 1 : 0).ToString());

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult<MinionJob>();
		}
	}
}
