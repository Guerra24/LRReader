namespace LRReader.Shared.Models.Main
{
	public class ShinobuStatus : GenericApiResult
	{
		public int is_alive { get; set; }
		public int pid { get; set; }
	}
}
