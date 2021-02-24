namespace LRReader.Shared.Models.Main
{
	public class DatabaseCleanResult : GenericApiResult
	{
		public int deleted { get; set; }
		public int unlinked { get; set; }
	}
}
