using System.Net;
using LRReader.Shared.Converters;
using Newtonsoft.Json;

namespace LRReader.Shared.Models.Main
{
	public class GenericApiResult
	{
		public string operation { get; set; } = null!;
		[JsonConverter(typeof(BoolConverter))]
		public bool success { get; set; }
		public string? error { get; set; }
	}

	public class MinionJob : GenericApiResult
	{
		public int job { get; set; }
	}

	public class GenericApiResponse<T>
	{
		public T? Data { get; set; }
		public GenericApiResult? Error { get; set; }
		public bool OK { get; set; }
		public HttpStatusCode Code { get; set; }

		public string? Json { get; set; }
	}

	public class CategoryCreatedApiResult : GenericApiResult
	{
		public string category_id { get; set; } = null!;
	}

	public class TankoubonCreateApiResult : GenericApiResult
	{
		public string tankoubon_id { get; set; } = null!;
	}

}