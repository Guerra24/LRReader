using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Models.Main
{
	public class Archive
	{
		public string arcid { get; set; }
		public string isnew { get; set; }
		public string tags { get; set; }
		public string title { get; set; }

		public bool IsNewArchive()
		{
			if (string.IsNullOrEmpty(isnew))
				return false;
			if (isnew.Equals("none"))
				return false;
			if (isnew.Equals("block"))
				return true;
			return bool.Parse(isnew);
		}
	}

	public class ArchiveImages
	{
		public List<string> pages { get; set; }
	}

	public class ArchiveImageSet
	{
		public string LeftImage;
		public string RightImage;
	}
}
