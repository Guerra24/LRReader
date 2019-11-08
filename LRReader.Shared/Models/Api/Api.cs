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
		public int success { get; set; }
	}

	public class GenericApiResponse<T>
	{
		public T Data { get; set; }
		public GenericApiError Error { get; set; }
		public bool OK { get; set; }
	}

	public class ShinobuStatus
	{
		public int is_alive { get; set; }
		public string operation { get; set; }
		public int pid { get; set; }
	}
}