using LRReader.Shared.Internal;
#if WINDOWS_UWP
using Microsoft.AppCenter.Crashes;
#endif
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace LRReader.Shared.Models.Main
{
	public class Archive : IEquatable<Archive>
	{
		[AllowNull]
		public string arcid { get; set; }
		[JsonConverter(typeof(ArchiveNewConverter))]
		public bool isnew { get; set; }
		public string? extension { get; set; }
		[AllowNull]
		public string tags { get; set; }
		[AllowNull]
		public string title { get; set; }
		public int pagecount { get; set; }
		public int progress { get; set; }
		[JsonIgnore]
		[AllowNull]
		public string TagsClean { get; set; }
		[JsonIgnore]
		public List<string> TagsList { get; set; } = new List<string>();
		[JsonIgnore]
		public ObservableCollection<ArchiveTagsGroup> TagsGroups { get; set; } = new ObservableCollection<ArchiveTagsGroup>();

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context) => UpdateTags();

		public void UpdateTags()
		{
			TagsClean = "";
			if (tags == null) // TODO: v0.7.7 returns null tags when there are no plugins enabled.
				tags = ""; // Use empty string as fallback instead
			var separatedTags = tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var s in separatedTags)
				TagsClean += s.Substring(Math.Max(s.IndexOf(':') + 1, 0)).Trim() + ", ";
			TagsClean = TagsClean.Trim().TrimEnd(',');
			TagsList.Clear();
			foreach (var s in separatedTags)
				TagsList.Add(s.Trim());

			// TODO: Apparently we can crash here with a COMException
			try
			{
				TagsGroups.Clear();
			}
			catch (Exception e)
			{
				// Drop original collection, can cause more COMExceptions
				TagsGroups = new ObservableCollection<ArchiveTagsGroup>();
#if WINDOWS_UWP
				Crashes.TrackError(e);
#endif
			}
			var tmp = new List<ArchiveTagsGroup>();
			foreach (var s in separatedTags)
			{
				var parts = s.Trim().Split(new char[] { ':' }, 2);
				var @namespace = parts.Length == 2 ? parts[0] : "other";
				var group = tmp.FirstOrDefault(tg => tg.Namespace.Equals(@namespace));
				if (group == null)
					group = AddTagsGroup(tmp, @namespace);
				var tag = parts[parts.Length - 1];
				if (parts[0].Equals("date_added"))
					if (long.TryParse(tag, out long unixTime))
						tag = Util.UnixTimeToDateTime(unixTime).ToString();
				group.Tags.Add(new ArchiveTagsGroupTag { FullTag = s.Trim(), Tag = tag, Namespace = @namespace });
			}
			tmp.Sort((a, b) => string.Compare(a.Namespace, b.Namespace));
			var c = tmp.Find(g => g.Namespace.Equals("other"));
			if (c != null)
			{
				tmp.Remove(c);
				tmp.Add(c);
			}
			try
			{
				tmp.ForEach(g =>
				{
					g.Namespace = g.Namespace.UpperFirstLetter().Replace('_', ' ');
					TagsGroups.Add(g);
				});
			}
			catch (Exception e)
			{
				// Handle damaged collection just in case
#if WINDOWS_UWP
				Crashes.TrackError(e);
#endif
			}
		}

		private ArchiveTagsGroup AddTagsGroup(List<ArchiveTagsGroup> list, string @namespace)
		{
			var group = new ArchiveTagsGroup() { Namespace = @namespace };
			list.Add(group);
			return group;
		}

		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;
			if (obj is Archive other)
			{
				if (other == this)
					return true;
				return arcid.Equals(other.arcid);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return arcid.GetHashCode();
		}

		public bool Equals(Archive other)
		{
			if (other is null)
				return false;
			if (other == this)
				return true;
			return arcid.Equals(other.arcid);
		}
	}

	public class ArchiveImages
	{
		[AllowNull]
		public List<string> pages { get; set; }
	}

	public class ReaderImageSet
	{
		[AllowNull]
		public string LeftImage { get; set; }
		public string? RightImage { get; set; }
		public int Page { get; set; }
		public bool TwoPages { get; set; }
		public double Width;
		public double Height;
	}

	public class ImagePageSet
	{

		public string Image { get; set; }
		public int Page { get; set; }

		public ImagePageSet(string image, int page)
		{
			this.Image = image;
			this.Page = page;
		}

		public override bool Equals(object obj)
		{
			return obj is ImagePageSet set &&
				   Image.Equals(set.Image);
		}

		public override int GetHashCode()
		{
			return Image.GetHashCode();
		}
	}

	public class ArchiveSearch
	{
		[AllowNull]
		public List<Archive> data { get; set; }
		public int draw { get; set; }
		public int recordsFiltered { get; set; }
		public int recordsTotal { get; set; }
	}

	public class ArchiveTagsGroup
	{
		[AllowNull]
		public string Namespace { get; set; }
		public List<ArchiveTagsGroupTag> Tags { get; set; }

		public ArchiveTagsGroup()
		{
			Tags = new List<ArchiveTagsGroupTag>();
		}
	}

	public class ArchiveTagsGroupTag
	{
		[AllowNull]
		public string FullTag;
		[AllowNull]
		public string Tag;
		[AllowNull]
		public string Namespace;
	}

	public class ArchiveCategories : GenericApiResult
	{
		[AllowNull]
		public List<Category> categories { get; set; }
	}

	public class DeleteArchiveResult : GenericApiResult
	{
		[AllowNull]
		public string id { get; set; }
		[AllowNull]
		public string filename { get; set; }
	}

	public class ArchiveHit
	{
		[AllowNull]
		public string Left { get; set; }
		[AllowNull]
		public string Right { get; set; }

		public override bool Equals(object obj) => obj is ArchiveHit hit &&
				   Left.Equals(hit.Left) && Right.Equals(hit.Right);

		public override int GetHashCode() => Left.GetHashCode() + Right.GetHashCode();
	}
}
