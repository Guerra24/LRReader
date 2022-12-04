using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using LRReader.Shared.Services;
using Newtonsoft.Json;

namespace LRReader.Shared.Models.Main
{
	public class ServerProfile : ObservableObject
	{
		public int Version { get; set; }
		public string UID { get; set; }
		public string Name { get; set; }
		public string ServerAddress { get; set; }
		public string ServerApiKey { get; set; }
		public List<BookmarkedArchive> Bookmarks { get; set; }
		public bool AcceptedDisclaimer { get; set; }
		public List<ArchiveHit> MarkedAsNonDuplicated { get; set; }
		public int CacheTimestamp { get; set; }

		[JsonIgnore]
		public bool HasApiKey
		{
			get => !string.IsNullOrEmpty(ServerApiKey) || !Service.Api.ServerInfo.has_password;
		}

		[JsonIgnore]
		public string ServerAddressBrowser => ServerAddress.TrimEnd('/');


		public ServerProfile(string name, string address, string key)
		{
			Name = name;
			ServerAddress = address;
			ServerApiKey = key;
			UID = Guid.NewGuid().ToString();
			Bookmarks = new List<BookmarkedArchive>();
			MarkedAsNonDuplicated = new List<ArchiveHit>();
			CacheTimestamp = -1;
			Version = 2;
		}

		public void Update()
		{
			OnPropertyChanged(string.Empty);
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object? obj)
		{
			return obj is ServerProfile profile && UID.Equals(profile.UID);
		}

		public override int GetHashCode()
		{
			return UID.GetHashCode();
		}
	}

	public class BookmarkedArchive : ObservableObject
	{
		public string archiveID { get; set; }
		public int page { get; set; }
		public int totalPages { get; set; }

		public BookmarkedArchive(string id)
		{
			archiveID = id;
		}

		public void Update()
		{
			OnPropertyChanged(string.Empty);
		}

		public override bool Equals(object? obj)
		{
			return obj is BookmarkedArchive archive && archiveID == archive.archiveID;
		}

		public override int GetHashCode()
		{
			return archiveID.GetHashCode();
		}

		[JsonIgnore]
		public int BookmarkProgressDisplay => page + 1;
		[JsonIgnore]
		public bool Bookmarked => totalPages >= 0;
	}

}
