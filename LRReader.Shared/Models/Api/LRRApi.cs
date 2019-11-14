using LRReader.Shared.Models.Main;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization;
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
		private string apiKey;

		private RestClient client;

		public LRRApi()
		{
			client = new RestClient();
			client.UseSerializer(() => new JsonNetSerializer());
		}

		public void RefreshSettings(ServerProfile profile)
		{
			client.BaseUrl = new Uri(profile.ServerAddress);
			if (profile.HasApiKey)
			{
				apiKey = profile.ServerApiKey;
				client.RemoveDefaultParameter("key");
				client.AddDefaultParameter("key", apiKey);
			}
			else
			{
				apiKey = "";
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

	public class JsonNetSerializer : IRestSerializer
	{
		public string Serialize(object obj) =>
			JsonConvert.SerializeObject(obj);

		public string Serialize(Parameter parameter) =>
			JsonConvert.SerializeObject(parameter.Value);

		public T Deserialize<T>(IRestResponse response) =>
			JsonConvert.DeserializeObject<T>(response.Content);

		public string[] SupportedContentTypes { get; } =
		{
			"application/json", "text/json", "text/x-json", "text/javascript", "*+json"
		};

		public string ContentType { get; set; } = "application/json";

		public DataFormat DataFormat { get; } = DataFormat.Json;
	}
}
