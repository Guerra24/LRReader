using System;
using LRReader.Shared.Converters;
using Newtonsoft.Json;

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

		[JsonIgnore]
		public bool _unauthorized;
	}

}
