namespace LRReader.Shared.Models.Main;

public class TagStats
{
	public string @namespace { get; set; } = null!;
	public string text { get; set; } = null!;
	public int weight { get; set; }

	public string GetNamespacedTag() => string.IsNullOrEmpty(@namespace) ? text : @namespace + ":" + text;
}
