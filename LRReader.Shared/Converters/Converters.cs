using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LRReader.Shared.Converters;

public class BoolConverter : JsonConverter<bool>
{
	public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions serializer)
	{
		writer.WriteStringValue(value ? "1" : "0");
	}

	public override bool Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions serializer)
	{
		if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
			return reader.GetBoolean();

		if (reader.TokenType == JsonTokenType.String)
		{
			return reader.GetString() switch
			{
				"0" => false,
				"1" => true,
				"True" => true,
				"False" => false,
				_ => false
			};
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			var val = reader.GetInt64();
			if (val == 0)
				return false;
			else
				return true;
		}
		throw new JsonException();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(bool);
	}
}

public class VersionConverter : JsonConverter<Version>
{

	private static readonly Regex onlyDigitOrDot = new Regex(@"[^\d|\.]+");

	public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions serializer)
	{
		writer.WriteStringValue(value.ToString());
	}

	public override Version? Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions serializer)
	{
		var cleanedString = onlyDigitOrDot.Replace(reader.GetString()!, "");
		if (cleanedString.Count(s => s == '.') > 4)
			return null;
		return new Version(cleanedString);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Version);
	}
}

public class ArchiveNewConverter : JsonConverter<bool>
{
	public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions serializer)
	{
		writer.WriteBooleanValue(value);
	}

	public override bool Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions serializer)
	{
		if (reader.TokenType == JsonTokenType.Null || reader.TokenType == JsonTokenType.None)
			return false;
		if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
			return reader.GetBoolean();

		if (reader.TokenType == JsonTokenType.String)
		{
			return reader.GetString() switch
			{
				"none" => false,
				"block" => true,
				"True" => true,
				"False" => false,
				_ => false
			};
		}

		throw new JsonException();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(bool);
	}
}

public class HtmlEncodingConverter : JsonConverter<string>
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(string);
	}

	public override string Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions serializer)
	{
		return WebUtility.HtmlDecode(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions serializer)
	{
		throw new NotImplementedException();
	}
}
