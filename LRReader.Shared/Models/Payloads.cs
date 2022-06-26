namespace LRReader.Shared.Models
{
	public class DownloadPayload
	{
		public byte[] Data { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Type { get; set; } = null!;
	}
}
