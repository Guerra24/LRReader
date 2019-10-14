using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Providers
{
	public class ImagesProvider
	{
		public async Task<List<string>> LoadImages(Archive archive)
		{
			var client = SharedGlobal.LRRApi.GetClient();

			var rq = new RestRequest("api/extract");

			rq.AddParameter("id", archive.arcid);

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<ArchiveImages>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					return result.Data.pages;
				case HttpStatusCode.Unauthorized:
					SharedGlobal.EventManager.ShowError("API Error", result.Error.error);
					return null;
			}
			return null;
		}
	}
}
