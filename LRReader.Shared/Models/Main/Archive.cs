using LRReader.Shared.Converters;
using LRReader.Shared.Extensions;
#if WINDOWS_UWP
using Microsoft.AppCenter.Crashes;
#endif
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace LRReader.Shared.Models.Main
{
	public class Archive : IEquatable<Archive>
	{
		public string arcid { get; set; } = null!;
		[JsonConverter(typeof(ArchiveNewConverter))]
		public bool isnew { get; set; }
		public string extension { get; set; } = null!;
		public string tags { get; set; } = null!;
		public string title { get; set; } = null!;
		public int pagecount { get; set; }
		public int progress { get; set; }
		public long? lastreadtime { get; set; } // 0.9.0

		[JsonIgnore]
		public string LastReadTimeString => lastreadtime == null ? "" : DateTimeOffset.FromUnixTimeSeconds((long)lastreadtime).ToLocalTime().ToString();

		public long? size { get; set; } // dev

		[JsonIgnore]
		public string SizeString => size == null ? "" : string.Format("{0:n2} MB", size / 1024f / 1024f);

		public string? filename { get; set; } // dev

		[JsonIgnore]
		public string TagsClean { get; set; } = null!;
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
						tag = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime().ToString();
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

	public class ArchiveImages : MinionJob
	{
		public List<string> pages { get; set; } = null!;
	}

	public class ReaderImageSet
	{

		public string? LeftImage { get; set; }
		public string? RightImage { get; set; }
		public int Page { get; set; }
		public bool TwoPages { get; set; }
		public double Width;
		public double Height;
	}

	public class ImagePageSet
	{
		public string Id { get; set; }
		public string? Image { get; set; }
		public int Page { get; set; }

		public ImagePageSet(string id, string? image, int page)
		{
			this.Id = id;
			this.Image = image;
			this.Page = page;
		}

		public override bool Equals(object obj)
		{
			return obj is ImagePageSet set &&
				   (Image?.Equals(set.Image) ?? Page == set.Page);
		}

		public override int GetHashCode()
		{
			return Image?.GetHashCode() ?? Page;
		}
	}

	public class ArchiveSearch
	{
		public List<Archive> data { get; set; } = null!;
		public int draw { get; set; }
		public int recordsFiltered { get; set; }
		public int recordsTotal { get; set; }
	}

	public class ArchiveTagsGroup
	{
		public string Namespace { get; set; } = null!;
		public List<ArchiveTagsGroupTag> Tags { get; set; }

		public ArchiveTagsGroup()
		{
			Tags = new List<ArchiveTagsGroupTag>();
		}
	}

	public class ArchiveTagsGroupTag
	{
		public string FullTag = null!;

		public string Tag = null!;

		public string Namespace = null!;
	}

	public class ArchiveCategories : GenericApiResult
	{
		public List<Category> categories { get; set; } = null!;
	}

	public class DeleteArchiveResult : GenericApiResult
	{

		public string id { get; set; } = null!;

		public string filename { get; set; } = null!;
	}

	public class ArchiveHit
	{

		public string Left { get; set; } = null!;

		public string Right { get; set; } = null!;

		public override bool Equals(object obj) => obj is ArchiveHit hit &&
			((Left.Equals(hit.Left) && Right.Equals(hit.Right)) || (Left.Equals(hit.Right) && Right.Equals(hit.Left)));

		public override int GetHashCode() => Left.GetHashCode() + Right.GetHashCode();
	}

	public class ThumbnailRequest
	{
		public MinionJob? Job;
		public byte[]? Thumbnail;
	}
}
