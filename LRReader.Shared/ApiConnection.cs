using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
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

	public static class ApiExtentions
	{

		public async static Task<bool> GetResult(this IRestResponse request)
		{
			var result = await request.GetResultInternal<GenericApiResult>();

			if (!string.IsNullOrEmpty(request.ErrorMessage))
			{
				Service.Events.ShowNotification("Network Error", request.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return true;
			}
			else
			{
				Service.Events.ShowNotification(result.Error.operation, result.Error.error);
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
				Service.Events.ShowNotification("Network Error", request.ErrorMessage);
				return default(T);
			}
			if (result.OK)
			{
				return result.Data;
			}
			else
			{
				Service.Events.ShowNotification(result.Error.operation, result.Error.error);
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
				apiResponse.Error = new GenericApiResult { operation = "Error while decoding response" };
			apiResponse.Code = restResponse.StatusCode;
			return apiResponse;
		}

		public static async Task<GenericApiResult> GetError(this IRestResponse restResponse)
		{
			var error = await Task.Run(() =>
			{
				try
				{
					return JsonConvert.DeserializeObject<GenericApiResult>(restResponse.Content);
				}
				catch (Exception e)
				{
					var attachments = new ErrorAttachmentLog[]
					{
						ErrorAttachmentLog.AttachmentWithText(restResponse.Content, "error-response.txt")
					};
					Crashes.TrackError(e, attachments: attachments);
					return null;
				}
			});
			if (error == null)
				return new GenericApiResult { operation = $"Error code: {(int)restResponse.StatusCode} {restResponse.StatusDescription}", error = $"{restResponse.ResponseUri}" };
			switch (restResponse.StatusCode)
			{
				case HttpStatusCode.Unauthorized:
					error.operation = "Unauthorized";
					break;
				default:
					if (string.IsNullOrEmpty(error.operation))
						error.operation = "";
					break;
			}
			return error;
		}
	}

}
