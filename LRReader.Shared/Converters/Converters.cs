using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LRReader.Shared.Converters;

public class BoolConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value is not null)
			writer.WriteValue((bool)value ? "1" : "0");
	}

	public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.ValueType == typeof(bool))
			return reader.Value!;

		if (reader.ValueType == typeof(string))
		{
			if (reader.Value!.Equals("0"))
				return false;
			if (reader.Value!.Equals("1"))
				return true;
			if (bool.TryParse(reader.Value as string, out bool result))
				return result;
		}

		if (reader.ValueType == typeof(short) || reader.ValueType == typeof(int) || reader.ValueType == typeof(long))
		{
			var val = (long)reader.Value!;
			if (val == 0)
				return false;
			else
				return true;
		}
		throw new JsonReaderException();
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
		if (reader.ValueType == null || reader.Value == null)
			return false;
		if (reader.ValueType == typeof(string))
		{
			if (reader.Value!.Equals("none"))
				return false;
			if (reader.Value!.Equals("block"))
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

public class HtmlEncodingConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(string);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		return WebUtility.HtmlDecode((string)reader.Value!);
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}