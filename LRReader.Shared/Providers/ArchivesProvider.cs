using LRReader.Shared.Internal;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public static class ArchivesProvider
	{

		public static async Task<List<Archive>> GetArchives()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archives");

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<List<Archive>>(r);

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

		public static async Task<Archive> GetArchive(string id)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archives/{id}/metadata");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<Archive>(r);

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

		public static async Task<byte[]> GetThumbnail(string id)
		{
			var client = SharedGlobal.LRRApi.GetClient();

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

		public static async Task<List<string>> ExtractArchive(string id)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archives/{id}/extract");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecutePostAsync(rq);

			var result = await LRRApi.GetResult<ArchiveImages>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			if (result.OK)
			{
				return result.Data.pages;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return null;
			}
		}

		public static async Task<DownloadPayload> DownloadArchive(string id)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archives/{id}/download");
			rq.AddParameter("id", id, ParameterType.UrlSegment);

			var r = await client.ExecuteGetAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
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
					var error = await LRRApi.GetError(r);
					SharedGlobal.EventManager.ShowError(error.title, error.error);
					return null;
			}
		}

		public static async Task<bool> ClearNewArchive(string id)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archives/{id}/isnew", Method.DELETE);
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
				return result.Data.success;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}

		public static async Task<bool> UpdateArchive(string id, string title = "", string tags = "")
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archives/{id}/metadata", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddQueryParameter("title", title);
			rq.AddQueryParameter("tags", tags);

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

		public static async Task<byte[]> GetImage(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			var client = SharedGlobal.LRRApi.GetClient();

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
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/archives/{id}/progress/{progress}", Method.PUT);
			rq.AddParameter("id", id, ParameterType.UrlSegment);
			rq.AddParameter("progress", progress, ParameterType.UrlSegment);

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
