using Newtonsoft.Json;

namespace LRReader.Shared.Models.Main
{
	public class ShinobuStatus : GenericApiResult
	{
		[JsonConverter(typeof(BoolConverter))]
		public bool is_alive { get; set; }
		public int pid { get; set; }
	}
}
