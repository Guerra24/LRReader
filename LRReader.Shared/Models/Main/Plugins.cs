using LRReader.Shared.Converters;
using System.Text.Json.Serialization;

namespace LRReader.Shared.Models.Main
{

	public enum PluginParameterType
	{
		Bool, String, Int
	}

	public class PluginParameter
	{
		public string? name { get; set; }
		public string desc { get; set; } = null!;
		[JsonConverter(typeof(JsonStringEnumConverter<PluginParameterType>))]
		public PluginParameterType type { get; set; }
	}

	public enum PluginType
	{
		Login, Metadata, Script, All
	}

	public class Plugin : IJsonOnDeserialized
	{

		public string author { get; set; } = null!;
		public string description { get; set; } = null!;
		public string icon { get; set; } = null!;
		public string name { get; set; } = null!;
		public string @namespace { get; set; } = null!;
		public string oneshot_arg { get; set; } = null!;
		public List<PluginParameter> parameters { get; set; } = null!;
		[JsonConverter(typeof(JsonStringEnumConverter<PluginType>))]
		public PluginType type { get; set; }
		public string version { get; set; } = null!;
		public string? login_from { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool hidden { get; set; }
		public int priority { get; set; }
		public string? installed_registry { get; set; }
		public string? installed_version { get; set; }
		public string? installed_sha256 { get; set; }


		public bool HasArg { get; set; }

		void IJsonOnDeserialized.OnDeserialized()
		{
			HasArg = !string.IsNullOrEmpty(oneshot_arg);
			if (HasArg)
				oneshot_arg = oneshot_arg.Replace("<br/>", "\n");
		}

		public override string ToString()
		{
			return name;
		}
	}

	public class PluginResultData
	{
		public string new_tags { get; set; } = null!;
	}

	public class UsePluginResult : GenericApiResult
	{
		public PluginResultData data { get; set; } = null!;
		public string type { get; set; } = null!;
	}

	public class PluginInstall
	{
		public string @namespace { get; set; } = null!;
		public string registry { get; set; } = null!;
		public string? version { get; set; }
		public bool forced { get; set; }
	}

	public class PluginInstallResult : GenericApiResult
	{
		public string name { get; set; } = null!;
		public string @namespace { get; set; } = null!;
		public string version { get; set; } = null!;
		public string installed_registry { get; set; } = null!;
		public string installed_sha256 { get; set; } = null!;
	}

}
