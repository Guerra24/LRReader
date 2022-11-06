using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LRReader.Shared.Models.Main
{

	public enum PluginParameterType
	{
		Bool, String, Int
	}

	public class PluginParameter
	{

		public string desc { get; set; } = null!;
		[JsonConverter(typeof(StringEnumConverter))]
		public PluginParameterType type { get; set; }
	}

	public enum PluginType
	{
		Login, Metadata, Script, All
	}

	public class Plugin
	{

		public string author { get; set; } = null!;
		public string description { get; set; } = null!;
		public string icon { get; set; } = null!;
		public string name { get; set; } = null!;
		public string @namespace { get; set; } = null!;
		public string oneshot_arg { get; set; } = null!;
		public List<PluginParameter> parameters { get; set; } = null!;
		[JsonConverter(typeof(StringEnumConverter))]
		public PluginType type { get; set; }
		public string version { get; set; } = null!;
		public string? login_from { get; set; }

		public bool HasArg { get; set; }

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
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

	public class PluginParameterTypeConverter : JsonConverter
	{

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			switch (reader.Value?.ToString())
			{
				case "string":
					return PluginParameterType.String;
				case "bool":
					return PluginParameterType.Bool;
				default:
					throw new JsonReaderException($"Type not supported: {reader.Value}");
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Version);
		}
	}

}
