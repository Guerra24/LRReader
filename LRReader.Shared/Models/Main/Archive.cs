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

			var result = LRRApi.GetResult<ApiResult>(r);

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
}
