using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace LRReader.Shared.Models.Main
{
	public class Archive
	{
		public string arcid { get; set; }
		public string isnew { get; set; }
		public string tags { get; set; }
		public string title { get; set; }
		[JsonIgnore]
		public string TagsClean { get; set; }
		[JsonIgnore]
		public List<string> TagsList { get; set; } = new List<string>();
		[JsonIgnore]
		public ObservableCollection<ArchiveTagsGroup> TagsGroups { get; set; } = new ObservableCollection<ArchiveTagsGroup>();

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
			TagsClean = "";
			foreach (var s in tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				TagsClean += s.Substring(Math.Max(s.IndexOf(':') + 1, 0)).Trim() + ", ";
			}
			TagsClean = TagsClean.Trim().TrimEnd(',');
			TagsList.Clear();
			foreach (var s in tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				TagsList.Add(s.Trim());
			}
			TagsGroups.Clear();
			var tmp = new List<ArchiveTagsGroup>();
			foreach (var s in tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var parts = s.Trim().Split(new char[] { ':' }, 2);
				ArchiveTagsGroup group = null;
				var @namespace = parts.Length == 2 ? parts[0] : "other";
				group = tmp.FirstOrDefault(tg => tg.Namespace.Equals(@namespace));
				if (group == null)
					group = AddTagsGroup(tmp, @namespace);
				group.Tags.Add(new ArchiveTagsGroupTag { FullTag = s.Trim(), Tag = parts[parts.Length - 1] });
			}
			tmp.Sort((a, b) => string.Compare(a.Namespace, b.Namespace));
			tmp.ForEach(e => TagsGroups.Add(e));
		}

		private ArchiveTagsGroup AddTagsGroup(List<ArchiveTagsGroup> list, string @namespace)
		{
			var group = new ArchiveTagsGroup() { Namespace = @namespace };
			list.Add(group);
			return group;
		}
	}

	public class ArchiveImages
	{
		public List<string> pages { get; set; }
	}

	public class ArchiveImageSet
	{
		public string LeftImage { get; set; }
		public string RightImage { get; set; }
	}

	public class ArchiveSearch
	{
		public List<Archive> data { get; set; }
		public int draw { get; set; }
		public int recordsFiltered { get; set; }
		public int recordsTotal { get; set; }
	}

	public class ArchiveTagsGroup
	{
		public string Namespace { get; set; }
		public List<ArchiveTagsGroupTag> Tags { get; set; }

		public ArchiveTagsGroup()
		{
			Tags = new List<ArchiveTagsGroupTag>();
		}
	}

	public class ArchiveTagsGroupTag
	{
		public string FullTag;
		public string Tag;
	}
}
