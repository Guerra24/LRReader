using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using LRReader.Shared.Converters;
using Sentry;

namespace LRReader.Shared.Models.Main
{
	public class Archive : IEquatable<Archive>, IJsonOnDeserialized
	{
		public string arcid { get; set; } = null!;
		[JsonConverter(typeof(ArchiveNewConverter))]
		public bool isnew { get; set; }
		public string extension { get; set; } = null!;
		public string tags { get; set; } = null!;
		public string title { get; set; } = null!;
		public int pagecount { get; set; }
		public int progress { get; set; }
		public long lastreadtime { get; set; }

		[JsonIgnore]
		public string LastReadTimeString => DateTimeOffset.FromUnixTimeSeconds(lastreadtime).DateTime.ToLocalTime().ToString();

		public long size { get; set; }

		public string filename { get; set; } = null!;

		public string summary { get; set; } = null!;

		[JsonIgnore]
		public string TagsClean { get; set; } = null!;
		[JsonIgnore]
		public ObservableCollection<ArchiveTagsGroup> TagsGroups { get; set; } = new();
		[JsonIgnore]
		public bool IsTank => extension.EndsWith("tank");
		[JsonIgnore]
		public float Rating = -1;
		[JsonIgnore]
		private List<ArchiveTagsGroup> VirtualTags { get; set; } = new();

		void IJsonOnDeserialized.OnDeserialized()
		{
			UpdateTags();
			if (progress == 0)
				progress = 1;
		}

		public void UpdateTags()
		{
			TagsClean = "";
			if (tags == null) // TODO: v0.7.7 returns null tags when there are no plugins enabled.
				tags = ""; // Use empty string as fallback instead
			foreach (var s in tags.Split([','], StringSplitOptions.RemoveEmptyEntries))
				TagsClean += s.Substring(Math.Max(s.IndexOf(':') + 1, 0)).Trim() + ", ";
			TagsClean = TagsClean.Trim().TrimEnd(',');

			// TODO: Apparently we can crash here with a COMException
			try
			{
				TagsGroups.Clear();
			}
			catch (Exception e)
			{
				// Drop original collection, can cause more COMExceptions
				TagsGroups = new ObservableCollection<ArchiveTagsGroup>();
				SentrySdk.CaptureException(e);
			}
			BuildVirtualTags();
			try
			{
				VirtualTags.ForEach(g => TagsGroups.Add(g));
			}
			catch (Exception e)
			{
				// Handle damaged collection just in case
				SentrySdk.CaptureException(e);
			}
		}

		public void BuildVirtualTags()
		{
			VirtualTags.Clear();
			foreach (var s in tags.Split([','], StringSplitOptions.RemoveEmptyEntries))
			{
				var parts = s.Trim().Split([':'], 2);
				var @namespace = parts.Length == 2 ? parts[0] : "other";
				var tag = parts[parts.Length - 1];
				var group = AddOrGetNamespace(@namespace);
				if ((@namespace.Equals("date_added") || @namespace.Equals("timestamp")) && long.TryParse(tag, out var unixTime))
					tag = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime.ToLocalTime().ToString();
				if (@namespace.Equals("rating", StringComparison.InvariantCultureIgnoreCase))
					Rating = tag.Count(t => t.Equals('⭐'));
				group.Tags.Add(new ArchiveTagsGroupTag { FullTag = s.Trim(), Tag = tag, Namespace = @namespace });
			}
		}

		public string BuildStringTags()
		{
			var builder = new StringBuilder();
			foreach (var @namespace in VirtualTags)
				foreach (var tag in @namespace.Tags)
				{
					builder.Append(tag.FullTag);
					builder.Append(", ");
				}
			return builder.ToString().Trim([',', ' ']);
		}

		private ArchiveTagsGroup AddOrGetNamespace(string @namespace)
		{
			var group = VirtualTags.FirstOrDefault(tg => tg.Namespace.Equals(@namespace));
			if (group == null)
			{
				group = new ArchiveTagsGroup() { Namespace = @namespace };
				VirtualTags.Add(group);
				VirtualTags.Sort((a, b) =>
				{
					if (a.Namespace.Equals("other"))
						return 1;
					else if (b.Namespace.Equals("other"))
						return -1;
					else
						return string.Compare(a.Namespace, b.Namespace);
				});
			}
			return group;
		}

		public void RemoveNamespace(string @namespace)
		{
			VirtualTags.RemoveAll(group => group.Namespace.Equals(@namespace));
		}

		public void SetRating(int rating)
		{
			Rating = rating;
			var @namespace = AddOrGetNamespace("rating");
			if (rating > 0)
			{
				var tag = new string('⭐', rating);
				if (@namespace.Tags.Any())
				{
					var virtualTag = @namespace.Tags.First();
					virtualTag.Tag = tag;
					virtualTag.FullTag = $"rating:{tag}";
				}
				else
				{
					@namespace.Tags.Add(new ArchiveTagsGroupTag { FullTag = $"rating:{tag}", Tag = tag, Namespace = "rating" });
				}
			}
			else
			{
				@namespace.Tags.Clear();
				RemoveNamespace("rating");
			}
			tags = BuildStringTags();
		}

		public override bool Equals(object? obj)
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

		public bool Equals(Archive? other)
		{
			if (other is null)
				return false;
			if (other == this)
				return true;
			return arcid.Equals(other.arcid);
		}
	}

	public class ArchiveImages : MinionJob // Remove once support for 0.9.40 is removed
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

		public override bool Equals(object? obj)
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
		public List<ArchiveIdOnly> data { get; set; } = null!;
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

		public override bool Equals(object? obj) => obj is ArchiveHit hit &&
			((Left.Equals(hit.Left) && Right.Equals(hit.Right)) || (Left.Equals(hit.Right) && Right.Equals(hit.Left)));

		public override int GetHashCode() => Left.GetHashCode() + Right.GetHashCode();
	}

	public class ThumbnailRequest
	{
		public MinionJob? Job;
		public byte[]? Thumbnail;
	}

	public class ArchiveIdOnly : IEquatable<ArchiveIdOnly>
	{
		public string arcid { get; set; } = null!;

		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;
			if (obj is ArchiveIdOnly other)
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

		public bool Equals(ArchiveIdOnly? other)
		{
			if (other is null)
				return false;
			if (other == this)
				return true;
			return arcid.Equals(other.arcid);
		}
	}
}
