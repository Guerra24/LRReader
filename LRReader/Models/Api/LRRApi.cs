using LRReader.Models.Main;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LRReader.Models.Api
{
	public class LRRApi
	{
		private string apiKey;

		private RestClient client;

		public LRRApi()
		{
			client = new RestClient();
		}

		public void RefreshSettings()
		{
			ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
			string serverAddress = roamingSettings.Values["ServerAddress"] as string;
			client.BaseUrl = new Uri(serverAddress);
			apiKey = roamingSettings.Values["ApiKey"] as string;
			if (!string.IsNullOrEmpty(apiKey))
			{
				Debug.WriteLine("Using key");
				client.RemoveDefaultParameter("key");
				client.AddDefaultParameter("key", apiKey);
			}
		}

		public RestClient GetClient()
		{
			return client;
		}

		public static ApiResponse<T> GetResult<T>(IRestResponse restResponse)
		{
			var apiResponse = new ApiResponse<T>();
			if (restResponse.StatusCode == HttpStatusCode.OK)
			{
				apiResponse.Data = JsonConvert.DeserializeObject<T>(restResponse.Content);
			}
			else
			{
				apiResponse.Error = JsonConvert.DeserializeObject<ApiError>(restResponse.Content);
			}
			return apiResponse;
		}

	}

	public class ApiResponse<T>
	{
		public T Data { get; set; }
		public ApiError Error { get; set; }
	}

	public class ApiError
	{
		public string error { get; set; }
	}
}
