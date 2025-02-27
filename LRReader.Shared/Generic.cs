using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
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
	public static JsonSerializerOptions Options = new()
	{
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.AllowReadingFromString,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		IncludeFields = true,
		TypeInfoResolver = JsonSourceGenerationContext.Default,
	};
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, IncludeFields = true)]
[JsonSerializable(typeof(Archive))]
[JsonSerializable(typeof(ArchiveImages))]
[JsonSerializable(typeof(ArchiveSearch))]
[JsonSerializable(typeof(ArchiveCategories))]
[JsonSerializable(typeof(Category))]
[JsonSerializable(typeof(Plugin))]
[JsonSerializable(typeof(PluginResultData))]
[JsonSerializable(typeof(MinionJob))]
[JsonSerializable(typeof(ServerProfile))]
[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(AppState))]
[JsonSerializable(typeof(TagStats))]
[JsonSerializable(typeof(Tankoubon))]
[JsonSerializable(typeof(TankoubonsList))]
[JsonSerializable(typeof(TankoubonsItem))]
[JsonSerializable(typeof(GenericApiResult))]
[JsonSerializable(typeof(DeleteArchiveResult))]
[JsonSerializable(typeof(CategoryCreatedApiResult))]
[JsonSerializable(typeof(TankoubonCreateApiResult))]
[JsonSerializable(typeof(DatabaseCleanResult))]
[JsonSerializable(typeof(UsePluginResult))]
[JsonSerializable(typeof(MinionStatus))]
[JsonSerializable(typeof(ShinobuStatus))]
[JsonSerializable(typeof(ShinobuRescan))]
[JsonSerializable(typeof(ArchiveTankoubons))]
[JsonSerializable(typeof(List<Archive>))]
[JsonSerializable(typeof(List<BookmarkedArchive>))]
[JsonSerializable(typeof(ObservableCollection<ServerProfile>))]
[JsonSerializable(typeof(Dictionary<string, Archive>))]
[JsonSerializable(typeof(ConcurrentDictionary<string, Archive>))]
[JsonSerializable(typeof(List<TagStats>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<Plugin>))]
[JsonSerializable(typeof(List<ArchiveCategories>))]
[JsonSerializable(typeof(List<Category>))]
[JsonSerializable(typeof(UpdateChangelog))]
[JsonSerializable(typeof(VersionSupportedRange))]
public partial class JsonSourceGenerationContext : JsonSerializerContext
{

}