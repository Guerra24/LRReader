using LRReader.Shared.Converters;
using System.Text.Json.Serialization;

namespace LRReader.Shared.Models.Main
{
	public class ServerInfo
	{
		[JsonConverter(typeof(HtmlEncodingConverter))]
		public string name { get; set; } = null!;
		[JsonConverter(typeof(HtmlEncodingConverter))]
		public string motd { get; set; } = null!;
		[JsonConverter(typeof(VersionConverter))]
		public Version version { get; set; } = null!;
		public string version_name { get; set; } = null!;
		[JsonConverter(typeof(BoolConverter))]
		public bool has_password { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool debug_mode { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool nofun_mode { get; set; }
		public int archives_per_page { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool server_resizes_images { get; set; }
		public int cache_last_cleared { get; set; }
		public string? version_desc { get; set; }
		public int total_pages_read { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool server_tracks_progress { get; set; }
		public List<string> excluded_namespaces { get; set; } = ["source", "date_added"];

		[JsonIgnore]
		public bool _unauthorized;
	}

	public class BaseRegistry
	{
		public string name { get; set; } = null!;

		public RegistryType type { get; set; }
		public RegistryProvider? provider { get; set; }
		public string? url { get; set; }
		[JsonPropertyName("ref")]
		public string? gitRef { get; set; }
		public string? path { get; set; }
	}

	[JsonConverter(typeof(JsonStringEnumConverter<RegistryType>))]
	public enum RegistryType
	{
		[JsonStringEnumMemberName("git")]
		Git,
		[JsonStringEnumMemberName("local")]
		Local
	}

	[JsonConverter(typeof(JsonStringEnumConverter<RegistryProvider>))]
	public enum RegistryProvider
	{
		[JsonStringEnumMemberName("github")]
		Github,
		[JsonStringEnumMemberName("gitlab")]
		Gitlab,
		[JsonStringEnumMemberName("gitea")]
		Gitea
	}

	public class RegistryResult : GenericApiResult
	{
		public string id { get; set; } = null!;
	}

	public class RegistryDefaultResult : GenericApiResult
	{
		public string registry_id { get; set; } = null!;
	}

	public class RegistryMetadataResult : RegistryResult
	{
		public Registry registry { get; set; } = null!;
	}

	public class Registry : BaseRegistry
	{
		public string id { get; set; } = null!;
		public int created { get; set; }
		public int updated { get; set; }
	}

	public class RegistryUpdatedResult : RegistryResult
	{
		[JsonConverter(typeof(BoolConverter))]
		public bool index_cleared { get; set; }
	}

	public class RegistriesResult : GenericApiResult
	{
		public List<Registry> registries { get; set; } = [];
	}

}
