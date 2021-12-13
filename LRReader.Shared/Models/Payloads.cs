using System.Diagnostics.CodeAnalysis;

namespace LRReader.Shared.Models
{
	public class DownloadPayload
	{
		[NotNull]
		public byte[]? Data { get; set; }
		[NotNull]
		public string? Name { get; set; }
		[NotNull]
		public string? Type { get; set; }
	}
}
