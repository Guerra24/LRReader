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
			CategoriesProvider = new CategoriesProvider();
		}

		public static ArchivesProvider ArchivesProvider { get; }
		public static ImagesProvider ImagesProvider { get; }
		public static ServerProvider ServerProvider { get; }
		public static CategoriesProvider CategoriesProvider { get; }

	}
}
