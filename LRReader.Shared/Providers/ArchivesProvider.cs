using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using CommunityToolkit.Mvvm.Messaging;
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

		public static async Task<bool> Validate()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives");

			var r = await client.ExecuteAsync(rq, Method.Head);

			if (r.StatusCode != HttpStatusCode.OK)
				return false;

			//var decoded = GetArchives();
			//return decoded != null;
			return true;
		}

		public static async Task<List<Archive>?> GetArchives()
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives");

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<List<Archive>>();
		}

		public static async Task<Archive?> GetArchive(string id)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/metadata");
			rq.AddUrlSegment("id", id);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<Archive>();
		}

		public static async Task<ArchiveCategories?> GetArchiveCategories(string id)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/categories");
			rq.AddUrlSegment("id", id);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<ArchiveCategories>();
		}

		public static async Task<ThumbnailRequest?> GetThumbnail(string id, bool noFallback = false, int page = 0)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/thumbnail");
			rq.AddUrlSegment("id", id);
			rq.AddQueryParameter("no_fallback", noFallback.ToString().ToLower());
			rq.AddQueryParameter("page", page.ToString());

			var r = await client.ExecuteGetAsync(rq);

			switch (r.StatusCode)
			{
				case HttpStatusCode.Accepted:
					return new ThumbnailRequest { Job = await r.GetResult<MinionJob>() };
				case HttpStatusCode.OK:
					return new ThumbnailRequest { Thumbnail = r.RawBytes };
				default:
					return null;
			}
		}

		public static async Task<ArchiveImages?> ExtractArchive(string id)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/files");
			rq.AddUrlSegment("id", id);

			var r = await client.ExecuteGetAsync(rq);

			return await r.GetResult<ArchiveImages>();
		}

		public static async Task<DownloadPayload?> DownloadArchive(string id)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/download");
			rq.AddUrlSegment("id", id);

			var r = await client.ExecuteGetAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("Network Error", r.ErrorMessage));
				return null;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					var download = new DownloadPayload();
					var header = r.ContentHeaders.First(h => h.Name?.Equals("Content-Disposition") ?? false).Value as string;
					var parms = header?.Split(';').Select(s => s.Trim());
					var natr = parms.First(s => s.StartsWith("filename"));
					var nameAndType = natr.Substring(natr.IndexOf("\"") + 1, natr.Length - natr.IndexOf("\"") - 2);

					download.Data = r.RawBytes!;
					download.Name = nameAndType.Substring(0, nameAndType.LastIndexOf("."));
					download.Type = nameAndType.Substring(nameAndType.LastIndexOf("."));
					return download;
				default:
					var error = await r.GetError();
					WeakReferenceMessenger.Default.Send(new ShowNotification(error.operation, error.error));
					return null;
			}
		}

		public static async Task<bool> ChangeThumbnail(string id, int page)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/thumbnail", Method.Put);
			rq.AddUrlSegment("id", id);
			rq.AddQueryParameter("page", page);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<bool> ClearNewArchive(string id)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/isnew", Method.Delete);
			rq.AddUrlSegment("id", id);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<bool> UpdateArchive(string id, string title = "", string tags = "")
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/metadata", Method.Put);
			rq.AddUrlSegment("id", id);
			rq.AddQueryParameter("title", title);
			rq.AddQueryParameter("tags", tags);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<DeleteArchiveResult?> DeleteArchive(string id)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}", Method.Delete);
			rq.AddUrlSegment("id", id);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult<DeleteArchiveResult>();
		}

		public static async Task<byte[]?> GetImage(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			var client = Api.Client;

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
			var client = Api.Client;

			var rq = new RestRequest("api/archives/{id}/progress/{progress}", Method.Put);
			rq.AddUrlSegment("id", id);
			rq.AddParameter("progress", progress, ParameterType.UrlSegment);

			var r = await client.ExecuteAsync(rq);

			return await r.GetResult();
		}

		public static async Task<MinionJob?> RegenerateThumbnails(bool force = false)
		{
			var client = Api.Client;

			var rq = new RestRequest("api/regen_thumbs");
			rq.AddQueryParameter("force", (force ? 1 : 0).ToString());

			var r = await client.ExecutePostAsync(rq);

			return await r.GetResult<MinionJob>();
		}
	}
}
