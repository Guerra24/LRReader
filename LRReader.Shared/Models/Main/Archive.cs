using LRReader.Shared.Internal;
using LRReader.Shared.Models.Api;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
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
		public string tagsClean { get; set; }

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			UpdateTags();
		}

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

		public void UpdateTags()
		{
			tagsClean = "";
			foreach (var s in tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				tagsClean += s.Substring(Math.Max(s.IndexOf(':') + 1, 0)).Trim() + ", ";
			}
			tagsClean.TrimEnd(',');
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

	public class ArchiveSearch
	{
		public List<Archive> data { get; set; }
		public int draw { get; set; }
		public int recordsFiltered { get; set; }
		public int recordsTotal { get; set; }
	}
}
