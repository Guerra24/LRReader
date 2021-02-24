using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LRReader.Shared.Models.Main
{

	public enum PluginParameterType
	{
		Bool, String, Int
	}

	public class PluginParameter
	{
		public string desc { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public PluginParameterType type { get; set; }
	}

	public enum PluginType
	{
		Login, Metadata, Script, All
	}

	public class Plugin
	{
		public string author { get; set; }
		public string description { get; set; }
		public string icon { get; set; }
		public string name { get; set; }
		public string @namespace { get; set; }
		public string oneshot_arg { get; set; }
		public List<PluginParameter> parameters { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public PluginType type { get; set; }
		public string version { get; set; }
		public string login_from { get; set; }

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
		public string new_tags { get; set; }
	}

	public class UsePluginResult : GenericApiResult
	{
		public PluginResultData data { get; set; }
		public string type { get; set; }
	}

	public class PluginParameterTypeConverter : JsonConverter
	{

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			switch (reader.Value.ToString())
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
