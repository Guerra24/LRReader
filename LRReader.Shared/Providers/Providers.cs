using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Providers
{
	public static class Providers
	{
		static Providers()
		{
			ArchivesProvider = new ArchivesProvider();
			ImagesProvider = new ImagesProvider();
			ServerProvider = new ServerProvider();
		}

		public static ArchivesProvider ArchivesProvider { get; set; }
		public static ImagesProvider ImagesProvider { get; set; }
		public static ServerProvider ServerProvider { get; set; }

	}
}
