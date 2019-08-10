using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Models.Main
{
	public class Archive
	{
		public string arcid { get; set; }
		public string isnew { get; set; }
		public string tags { get; set; }
		public string title { get; set; }
	}

	public class ArchiveImages
	{
		public List<string> pages { get; set; }
	}
}
