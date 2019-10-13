using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Models.Main
{
	public class TagStats
	{
		public string @namespace { get; set; }
		public string text { get; set; }
		public int weight { get; set; }

		public string GetNamespacedTag()
		{
			return string.IsNullOrEmpty(@namespace) ? text : @namespace + ":" + text;
		}
	}
}
