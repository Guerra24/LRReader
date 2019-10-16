using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Models.Main
{
	public class Archive
	{
		public string arcid { get; set; }
		public string isnew { get; set; }
		public string tags { get; set; }
		public string title { get; set; }

		public bool IsNewArchive()
		{
			if (string.IsNullOrEmpty(isnew))
				return false;
			if (isnew.Equals("none"))
				return false;
			if (isnew.Equals("block"))
				return true;
			return bool.Parse(isnew);
		}

		public async Task<bool> ClearNew()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/clear_new");

			rq.AddParameter("id", arcid);

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<GenericApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return false;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					return true;
				case HttpStatusCode.Unauthorized:
					SharedGlobal.EventManager.ShowError("API Error", result.Error.error);
					return false;
			}
			return false;
		}
		public async Task<DownloadPayload> DownloadArchive()
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/servefile");

			rq.AddParameter("id", arcid);

			var r = await client.ExecuteGetTaskAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			if (r.StatusCode == HttpStatusCode.OK)
			{
				var download = new DownloadPayload();
				var header = r.Headers.First(h => h.Name.Equals("Content-Disposition")).Value as string;
				var parms = header.Split(';').Select(s => s.Trim());
				var natr = parms.First(s => s.StartsWith("filename"));
				var nameAndType = natr.Substring(natr.IndexOf("\"") + 1, natr.Length - natr.IndexOf("\"") - 2);

				download.Data = r.RawBytes;
				download.Name = nameAndType.Substring(0, nameAndType.LastIndexOf("."));
				download.Type = nameAndType.Substring(nameAndType.LastIndexOf("."));
				return download;
			}
			return null;
		}
	}

	public class ArchiveImages
	{
		public List<string> pages { get; set; }
	}

	public class ArchiveImageSet
	{
		public string LeftImage;
		public string RightImage;
	}

	public class ArchiveSearch
	{
		public List<Archive> data { get; set; }
		public int draw { get; set; }
		public int recordsFiltered { get; set; }
		public int recordsTotal { get; set; }
	}
}
