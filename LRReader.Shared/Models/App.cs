using Newtonsoft.Json;
using System;

namespace LRReader.Shared.Models
{
	public interface ICustomTab
	{
		object CustomTabControl { get; set; }

		string CustomTabId { get; set; }

		bool IsClosable { get; set; }

		void Unload();

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

	public struct UpdateChangelog
	{
		public string Name { get; set; }
		public string Content { get; set; }
	}

	public class DeduplicatorMetadata
	{
	}
}
