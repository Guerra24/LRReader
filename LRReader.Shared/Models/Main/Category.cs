using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Models.Main
{

	public delegate Task DeleteCategory(Category category);

	public class Category
	{
		public List<string> archives { get; set; }
		public string id { get; set; }
		public string last_used { get; set; }
		public string name { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool pinned { get; set; }
		public string search { get; set; }

		public bool Unconfigured()
		{
			return string.IsNullOrEmpty(search) && archives.Count == 0;
		}

		[JsonIgnore]
		public DeleteCategory DeleteCategory;
	}

	public class AddNewCategory : Category
	{

	}

}
