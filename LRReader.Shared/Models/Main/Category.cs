using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Models.Main
{
	public class Category
	{
		public List<string> archives { get; set; }
		public string id { get; set; }
		public string last_used { get; set; }
		public string name { get; set; }
		[JsonConverter(typeof(BoolConverter))]
		public bool pinned { get; set; }
		public string search { get; set; }
	}

	public class AddNewCategory : Category
	{

	}
}
