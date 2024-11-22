using System.Collections.Generic;
using System.Text.Json.Serialization;
using LRReader.Shared.Converters;

namespace LRReader.Shared.Models.Main
{

	public class Category
	{

		public List<string> archives { get; set; } = null!;
		public string id { get; set; } = null!;
		public string name { get; set; } = null!;
		[JsonConverter(typeof(BoolConverter))]
		public bool pinned { get; set; }
		public string search { get; set; } = null!;

		public bool Unconfigured()
		{
			return string.IsNullOrEmpty(search) && archives.Count == 0;
		}

		public override string ToString()
		{
			return name;
		}

		public override bool Equals(object? obj)
		{
			if (obj is AddNewCategory)
				return false;
			return obj is Category category && id.Equals(category.id);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}

	public class AddNewCategory : Category
	{
		public override bool Equals(object? obj)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

}
