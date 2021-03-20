using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared
{
	public class ApiConnection
	{
		private RestClient client;

		public void RefreshSettings(ServerProfile profile)
		{
			client = new RestClient();
			client.UseNewtonsoftJson();
			client.BaseUrl = new Uri(profile.ServerAddress);
			client.UserAgent = "LRReader";
			if (!string.IsNullOrEmpty(profile.ServerApiKey))
			{
				var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(profile.ServerApiKey));
				client.AddDefaultHeader("Authorization", $"Bearer {base64Key}");
			}
		}

		public RestClient GetClient()
		{
			return client;
		}

	}

	public static class ApiExtentions
	{

		public async static Task<bool> GetResult(this IRestResponse request)
		{
			var result = await request.GetResultInternal<GenericApiResult>();

			if (!string.IsNullOrEmpty(request.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", request.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return true;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return false;
			}
		}
		public async static Task<bool> GetResultNoError(this IRestResponse request)
		{
			var result = await request.GetResultInternal<GenericApiResult>();
			if (!string.IsNullOrEmpty(request.ErrorMessage))
				return false;
			if (result.OK)
				return result.Data.success;
			else
				return false;
		}

		public async static Task<T> GetResult<T>(this IRestResponse request)
		{
			var result = await request.GetResultInternal<T>();

			if (!string.IsNullOrEmpty(request.ErrorMessage))
			{
				SharedGlobal.EventManager.ShowError("Network Error", request.ErrorMessage);
				return default(T);
			}
			if (result.OK)
			{
				return result.Data;
			}
			else
			{
				SharedGlobal.EventManager.ShowError(result.Error.title, result.Error.error);
				return default(T);
			}
		}

		public async static Task<T> GetResultNoError<T>(this IRestResponse request)
		{
			var result = await request.GetResultInternal<T>();
			if (!string.IsNullOrEmpty(request.ErrorMessage))
				return default(T);
			if (result.OK)
				return result.Data;
			else
				return default(T);
		}

		public static async Task<GenericApiResponse<T>> GetResultInternal<T>(this IRestResponse restResponse)
		{
			var apiResponse = new GenericApiResponse<T>();
			var data = await Task.Run(() =>
			{
				try
				{
					return JsonConvert.DeserializeObject<T>(restResponse.Content);
				}
				catch (Exception e)
				{
					Crashes.TrackError(e); // We are getting bad data from the instance, send stack trace
					return default;
				}
			});
			if (data != null)
				switch (restResponse.StatusCode)
				{
					case HttpStatusCode.OK:
						apiResponse.Data = data;
						apiResponse.OK = true;
						break;
					default:
						apiResponse.Error = await restResponse.GetError();
						break;
				}
			else
				apiResponse.Error = new GenericApiError { title = "Error while decoding response" };
			apiResponse.Code = restResponse.StatusCode;
			return apiResponse;
		}

		public static async Task<GenericApiError> GetError(this IRestResponse restResponse)
		{
			switch (restResponse.StatusCode)
			{
				case HttpStatusCode.Unauthorized:
					var error = await Task.Run(() => JsonConvert.DeserializeObject<GenericApiError>(restResponse.Content));
					error.title = "Unauthorized";
					return error;
				default:
					return new GenericApiError { title = $"Error code: {(int)restResponse.StatusCode} {restResponse.StatusDescription}", error = $"{restResponse.ResponseUri}" };
			}
		}
	}

}
