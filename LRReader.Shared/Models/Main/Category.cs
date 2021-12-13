using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LRReader.Shared.Models.Main
{

	public class Category
	{
		[AllowNull]
		public List<string> archives { get; set; }
		[AllowNull]
		public string id { get; set; }
		[AllowNull]
		public string last_used { get; set; }
		[AllowNull]
		public string name { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool pinned { get; set; }
		[AllowNull]
		public string search { get; set; }

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
