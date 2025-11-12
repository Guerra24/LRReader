using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
		public List<TabState> Tabs { get; set; } = new();
	}

	[JsonDerivedType(typeof(IdTabState), 0)]
	[JsonDerivedType(typeof(ArchiveTabState), 1)]
	[JsonDerivedType(typeof(SearchTabState), 2)]
	public class TabState(Tab tab)
	{
		[JsonConverter(typeof(JsonStringEnumConverter<Tab>))]
		public Tab Tab { get; set; } = tab;
	}

	public class IdTabState : TabState
	{
		public string Id { get; set; } = null!;

		public IdTabState(Tab tab, string id) : base(tab) => Id = id;
	}

	public class ArchiveTabState(string id, int? page, bool wasOpen) : IdTabState(Tab.Archive, id)
	{
		public int? Page { get; set; } = page;
		public bool WasOpen { get; set; } = wasOpen;
	}

	public class SearchTabState(SearchState search) : TabState(Tab.SearchResults)
	{
		public SearchState Search { get; set; } = search;
	}

	public class SearchState(string query, int page, bool @new, bool untagged, string sortBy, Order orderBy, string category)
	{
		public string Query { get; set; } = query;
		public int Page { get; set; } = page;
		public bool New { get; set; } = @new;
		public bool Untagged { get; set; } = untagged;
		public string SortBy { get; set; } = sortBy;
		[JsonConverter(typeof(JsonStringEnumConverter<Order>))]
		public Order OrderBy { get; set; } = orderBy;
		public string Category { get; set; } = category;
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
