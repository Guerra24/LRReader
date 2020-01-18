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

			var r = await client.ExecuteGetAsync(rq);

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
	}
}
