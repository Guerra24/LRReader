using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LRReader.Shared.Models.Main
{
	public class ServerProfile : ObservableObject
	{
		public int Version { get; set; }
		public string UID { get; set; }
		[AllowNull]
		public string Name { get; set; }
		[AllowNull]
		public string ServerAddress { get; set; }
		[AllowNull]
		public string ServerApiKey { get; set; }
		public List<BookmarkedArchive> Bookmarks { get; set; }
		public bool AcceptedDisclaimer { get; set; }
		public List<ArchiveHit> MarkedAsNonDuplicated { get; set; }

		[JsonIgnore]
		public bool HasApiKey
		{
			get => !string.IsNullOrEmpty(ServerApiKey) || !Service.Api.ServerInfo.has_password;
		}

		[JsonIgnore]
		public string ServerAddressBrowser => ServerAddress.TrimEnd('/');

		public ServerProfile()
		{
			UID = Guid.NewGuid().ToString();
			Bookmarks = new List<BookmarkedArchive>();
			MarkedAsNonDuplicated = new List<ArchiveHit>();
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
		[AllowNull]
		public string archiveID { get; set; }
		public int page { get; set; }
		public int totalPages { get; set; }

		public void Update()
		{
			OnPropertyChanged(string.Empty);
		}

		[JsonIgnore]
		public int BookmarkProgressDisplay => page + 1;
		[JsonIgnore]
		public bool Bookmarked => totalPages >= 0;
	}

}
