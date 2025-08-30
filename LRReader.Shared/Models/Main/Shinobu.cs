using LRReader.Shared.Converters;
using System.Text.Json.Serialization;

namespace LRReader.Shared.Models.Main
{
	public class ShinobuStatus : GenericApiResult
	{
		[JsonConverter(typeof(BoolConverter))]
		public bool is_alive { get; set; }
		public int pid { get; set; }
	}

	public class ShinobuRescan : GenericApiResult
	{
		public int new_pid { get; set; }
	}
}
