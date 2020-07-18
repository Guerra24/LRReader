using Newtonsoft.Json;
using System.Net;

namespace LRReader.Shared.Models.Api
{
	public class GenericApiError
	{
		public string title { get; set; }
		public string error { get; set; }
	}

	public class GenericApiResult
	{
		public string operation { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool success { get; set; }
		public string error { get; set; }
	}

	public class GenericApiResponse<T>
	{
		public T Data { get; set; }
		public GenericApiError Error { get; set; }
		public bool OK { get; set; }
		public HttpStatusCode Code;
	}

}