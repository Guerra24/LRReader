using System;
using LRReader.Shared.Converters;
using Newtonsoft.Json;

namespace LRReader.Shared.Models
{
	public interface ICustomTab : IDisposable
	{
		object CustomTabControl { get; set; }

		string CustomTabId { get; set; }

		bool IsClosable { get; set; }

		bool BackRequested();
	}

	public class ReleaseInfo
	{
		public string name { get; set; } = null!;
		public string body { get; set; } = null!;
		[JsonConverter(typeof(VersionConverter))]
		public Version version { get; set; } = null!;
		public string link { get; set; } = null!;
	}

	public class VersionSupportedRange
	{
		[JsonConverter(typeof(VersionConverter))]
		public Version minSupported { get; set; } = null!;
		[JsonConverter(typeof(VersionConverter))]
		public Version maxSupported { get; set; } = null!;
	}

	public class UpdateResult
	{
		public bool Result { get; set; }
		public int ErrorCode { get; set; }
		public string? ErrorMessage { get; set; }
	}

	public class CheckForUpdatesResult : UpdateResult
	{
		[JsonConverter(typeof(VersionConverter))]
		public Version Target { get; set; } = null!;
		public string Link { get; set; } = null!;
	}

	public record UpdateChangelog
	{
		public string Name { get; set; } = null!;
		public string Content { get; set; } = null!;
	}

	public class DeduplicatorMetadata
	{
	}
}
