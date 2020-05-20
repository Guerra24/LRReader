using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Models
{
	public class BoolConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((bool)value) ? "1" : "0");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return reader.Value.ToString() == "1";
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(bool);
		}
	}
}
