using ICSharpCode.SharpZipLib.BZip2;
using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared
{

	public static class ApiExtentions
	{

		public async static Task<bool> GetResult(this RestResponse request)
		{
			var result = await request.GetResultInternal<GenericApiResult>();

			if (!string.IsNullOrEmpty(request.ErrorMessage))
			{
				ShowNotification("Network Error", request.ErrorMessage);
				return false;
			}
			if (result.OK)
			{
				return true;
			}
			else
			{
				ShowNotification(result.Error?.operation ?? "", result.Error?.error ?? "");
				return false;
			}
		}
		public async static Task<bool> GetResultNoError(this RestResponse request)
		{
			var result = await request.GetResultInternal<GenericApiResult>();
			if (!string.IsNullOrEmpty(request.ErrorMessage))
				return false;
			if (result.OK)
				return result.Data?.success ?? false;
			else
				return false;
		}

		public async static Task<T?> GetResult<T>(this RestResponse request)
		{
			var result = await request.GetResultInternal<T>();

			if (!string.IsNullOrEmpty(request.ErrorMessage))
			{
				ShowNotification("Network Error", request.ErrorMessage);
				return default(T);
			}
			if (result.OK)
			{
				return result.Data;
			}
			else
			{
				ShowNotification(result.Error?.operation ?? "", result.Error?.error ?? "");
				return default(T);
			}
		}
		public async static Task<GenericApiResponse<T>?> GetResultComplete<T>(this RestResponse request)
		{
			var result = await request.GetResultInternal<T>();

			if (!string.IsNullOrEmpty(request.ErrorMessage))
			{
				ShowNotification("Network Error", request.ErrorMessage);
				return null;
			}
			if (result.OK)
			{
				return result;
			}
			else
			{
				ShowNotification(result.Error?.operation ?? "", result.Error?.error ?? "");
				return null;
			}
		}

		public async static Task<T?> GetResultNoError<T>(this RestResponse request)
		{
			var result = await request.GetResultInternal<T>();
			if (!string.IsNullOrEmpty(request.ErrorMessage))
				return default(T);
			if (result.OK)
				return result.Data;
			else
				return default(T);
		}

		public static async Task<GenericApiResponse<T>> GetResultInternal<T>(this RestResponse restResponse)
		{
			var apiResponse = new GenericApiResponse<T>();
			var data = await Task.Run(() =>
			{
				T? value = default;
				Exception? ex = null;
				try
				{
					if (restResponse.Content != null)
						value = JsonConvert.DeserializeObject<T>(restResponse.Content);
				}
				catch (Exception e)
				{
					/*var properties = new Dictionary<string, string>
					{
						{ "Path", restResponse.ResponseUri?.PathAndQuery ?? "" },
						{ "Code", restResponse.StatusCode.ToString() }
					};
					var json = CompressData(restResponse.Content);
					if (json == null)
					{
						Crashes.TrackError(e, properties);
						return default;
					}
					var attachments = new ErrorAttachmentLog[]
					{
						ErrorAttachmentLog.AttachmentWithBinary(json, "input.json.bz2", "application/x-bzip2")
					};
					Crashes.TrackError(e, properties, attachments);*/ // We are getting bad data from the instance, send stack trace
					ex = e;
				}
				return new Tuple<T?, Exception?>(value, ex);
			});
			if (data.Item1 != null)
				switch (restResponse.StatusCode)
				{
					case HttpStatusCode.OK:
						apiResponse.Data = data.Item1;
						apiResponse.OK = true;
						apiResponse.Json = restResponse.Content;
						break;
					default:
						apiResponse.Error = await restResponse.GetError();
						break;
				}
			else
				apiResponse.Error = new GenericApiResult { operation = "Error while decoding response", error = data.Item2?.Message };
			apiResponse.Code = restResponse.StatusCode;
			return apiResponse;
		}

		public static async Task<GenericApiResult> GetError(this RestResponse restResponse)
		{
			var error = await Task.Run(() =>
			{
				try
				{
					if (restResponse.Content != null)
						return JsonConvert.DeserializeObject<GenericApiResult>(restResponse.Content);
					else
						return default;
				}
				catch (Exception)
				{
					/*var properties = new Dictionary<string, string>
					{
						{ "Path", restResponse.ResponseUri?.PathAndQuery ?? "" },
						{ "Code", restResponse.StatusCode.ToString() }
					};
					var attachments = new ErrorAttachmentLog[]
					{
						ErrorAttachmentLog.AttachmentWithText(restResponse.Content, "error-response.txt")
					};
					Crashes.TrackError(e, properties, attachments);*/
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

		public static byte[]? CompressData(string? data)
		{
			if (data == null)
				return null;
			byte[] buffer = Encoding.UTF8.GetBytes(data);
			using (var compressed = new MemoryStream())
			{
				using (var bzip2Comp = new BZip2OutputStream(compressed))
				{
					bzip2Comp.Write(buffer, 0, buffer.Length);
				}
				return compressed.ToArray();
			}
		}

		private static void ShowNotification(string title, string content) => WeakReferenceMessenger.Default.Send(new ShowNotification(title, content));
	}

}
