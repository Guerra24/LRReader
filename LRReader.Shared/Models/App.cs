using LRReader.Shared.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using VersionConverter = LRReader.Shared.Converters.VersionConverter;

namespace LRReader.Shared.Models
{
	public interface ICustomTab : IDisposable
	{
		object? CustomTabControl { get; set; }

		string CustomTabId { get; set; }

		Tab Tab { get; set; }

		public TabState GetTabState();

		bool IsClosable { get; set; }

		bool BackRequested();
	}

	public class AppState
	{
		public List<TabState> Tabs { get; } = new();
		public string ProfileUID { get; set; } = null!;
	}

	[JsonDerivedType(typeof(IdTabState), 0)]
	[JsonDerivedType(typeof(ArchiveTabState), 1)]
	public class TabState
	{
		[JsonConverter(typeof(JsonStringEnumConverter<Tab>))]
		public Tab Tab { get; set; }

		public TabState(Tab tab) => Tab = tab;
	}

	public class IdTabState : TabState
	{
		public string Id { get; set; } = null!;

		public IdTabState(Tab tab, string id) : base(tab) => Id = id;
	}

	public class ArchiveTabState : IdTabState
	{
		public int? Page { get; set; }
		public bool WasOpen { get; set; }

		public ArchiveTabState(Tab tab, string id, int? page, bool wasOpen) : base(tab, id)
		{
			Page = page;
			WasOpen = wasOpen;
		}
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

	public class AotDictionaryHelper
	{
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type Type { get; }

		public AotDictionaryHelper([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
		{
			Type = type;
		}
	}
}
