using System.Diagnostics.CodeAnalysis;

namespace LRReader.Shared.Models.Main
{
	public class TagStats
	{
		[AllowNull]
		public string @namespace { get; set; }
		[AllowNull]
		public string text { get; set; }
		public int weight { get; set; }

		public string GetNamespacedTag()
		{
			return string.IsNullOrEmpty(@namespace) ? text : @namespace + ":" + text;
		}
	}
}
