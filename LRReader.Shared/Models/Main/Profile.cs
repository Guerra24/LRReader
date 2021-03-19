using LRReader.Shared.Internal;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

		[JsonIgnore]
		public bool HasApiKey
		{
			get => !string.IsNullOrEmpty(ServerApiKey) || !SharedGlobal.ServerInfo.has_password;
		}

		[JsonIgnore]
		public string ServerAddressBrowser => ServerAddress.TrimEnd('/');

		public ServerProfile()
		{
			UID = Guid.NewGuid().ToString();
			Bookmarks = new List<BookmarkedArchive>();
			Version = 1;
		}

		public void Update()
		{
			OnPropertyChanged(string.Empty);
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class BookmarkedArchive : ObservableObject
	{
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

	public enum ConflictMode
	{
		Local, Remote
	}
}
