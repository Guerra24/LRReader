using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

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
		public string name { get; set; }
		public string body { get; set; }
		[JsonConverter(typeof(VersionConverter))]
		public Version version { get; set; }
		public string link { get; set; }
	}

	public class VersionSupportedRange
	{
		[JsonConverter(typeof(VersionConverter))]
		public Version minSupported { get; set; }
		[JsonConverter(typeof(VersionConverter))]
		public Version maxSupported { get; set; }
	}

	public struct UpdateResult
	{
		public bool Result { get; set; }
		public int ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
	}

	public struct CheckForUpdatesResult
	{
		public bool Found { get; set; }
		[JsonConverter(typeof(VersionConverter))]
		public Version Target { get; set; }
		public string Link { get; set; }
	}

	public struct UpdateChangelog
	{
		public string Name { get; set; }
		public string Content { get; set; }
	}
}
