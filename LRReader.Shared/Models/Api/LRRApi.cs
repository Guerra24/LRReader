using LRReader.Shared.Models.Main;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Models.Api
{
	public class LRRApi
	{
		private RestClient client;

		public LRRApi()
		{
		}

		public void RefreshSettings(ServerProfile profile)
		{
			client = new RestClient();
			client.UseNewtonsoftJson();
			client.BaseUrl = new Uri(profile.ServerAddress);
			client.UserAgent = "LRReader";
			if (profile.HasApiKey)
			{
				var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(profile.ServerApiKey));
				client.AddDefaultHeader("Authorization", $"Bearer {base64Key}");
			}
		}

		public RestClient GetClient()
		{
			return client;
		}

		public static async Task<GenericApiResponse<T>> GetResult<T>(IRestResponse restResponse)
		{
			var apiResponse = new GenericApiResponse<T>();
			switch (restResponse.StatusCode)
			{
				case HttpStatusCode.OK:
					apiResponse.Data = await Task.Run(() => JsonConvert.DeserializeObject<T>(restResponse.Content));
					apiResponse.OK = true;
					break;
				default:
					apiResponse.Error = await GetError(restResponse);
					break;
			}
			return apiResponse;
		}

		public static async Task<GenericApiError> GetError(IRestResponse restResponse)
		{
			switch (restResponse.StatusCode)
			{
				case HttpStatusCode.Unauthorized:
					var error = await Task.Run(() => JsonConvert.DeserializeObject<GenericApiError>(restResponse.Content));
					error.title = "Unauthorized";
					return error;
				default:
					return new GenericApiError() { title = $"Error code: {(int)restResponse.StatusCode} {restResponse.StatusDescription}", error = $"{restResponse.ResponseUri}" };
			}
		}
	}

}
