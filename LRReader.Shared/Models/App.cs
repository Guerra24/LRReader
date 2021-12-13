using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

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
		[NotNull]
		public string? name { get; set; }
		[NotNull]
		public string? body { get; set; }
		[JsonConverter(typeof(VersionConverter))]
		[NotNull]
		public Version? version { get; set; }
		public string? link { get; set; }
	}

	public class VersionSupportedRange
	{
		[JsonConverter(typeof(VersionConverter))]
		[NotNull]
		public Version? minSupported { get; set; }
		[JsonConverter(typeof(VersionConverter))]
		[NotNull]
		public Version? maxSupported { get; set; }
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
		public Version? Target { get; set; }
		public string? Link { get; set; }
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
