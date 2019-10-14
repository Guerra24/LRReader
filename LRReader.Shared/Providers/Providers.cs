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
		}

		public static ArchivesProvider ArchivesProvider { get; set; }

	}
}
