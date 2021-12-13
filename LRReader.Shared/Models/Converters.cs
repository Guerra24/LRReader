using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace LRReader.Shared.Models
{
	public class BoolConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is not null)
				writer.WriteValue(((bool)value) ? "1" : "0");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			return reader.Value?.ToString() == "1";
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(bool);
		}
	}

	public class VersionConverter : JsonConverter
	{

		private static readonly Regex onlyDigitOrDot = new Regex(@"[^\d|\.]+");

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			writer.WriteValue((value as Version)?.ToString());
		}

		[return: MaybeNull]
		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var cleanedString = onlyDigitOrDot.Replace(reader.Value?.ToString(), "");
			if (cleanedString.Count(s => s == '.') > 4)
				return null;
			return new Version(cleanedString);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Version);
		}
	}

	public class ArchiveNewConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			writer.WriteValue(value);
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			if (reader.ValueType == typeof(string))
			{
				if (reader.Value?.Equals("none") ?? false)
					return false;
				if (reader.Value?.Equals("block") ?? false)
					return true;
				if (bool.TryParse(reader.Value as string, out bool result))
					return result;
			}
			if (reader.ValueType == typeof(bool))
				return reader.Value;
			throw new JsonReaderException();
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(bool);
		}
	}
}
