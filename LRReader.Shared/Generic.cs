using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace LRReader.Shared;

public delegate Task AsyncAction<T>(T obj);

public static class AsyncActionExtensions
{
	public static Task InvokeAsync<T>(this AsyncAction<T>? action, T obj)
	{
		return action?.Invoke(obj) ?? Task.CompletedTask;
	}
}

public static class JsonSettings
{
	public static JsonSerializerOptions Options = new() {
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.AllowReadingFromString,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		IncludeFields = true
	};
}