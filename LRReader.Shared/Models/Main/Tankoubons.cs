using System.Collections.Generic;

namespace LRReader.Shared.Models.Main
{
	public class Tankoubons
	{
		public List<string> archives { get; set; } = null!;
		public string id { get; set; } = null!;
		public string name { get; set; } = null!;
		public List<Archive>? full_data { get; set; }
	}

	public class TankoubonsList
	{
		public List<Tankoubons> result { get; set; } = null!;
		public int filtered { get; set; }
		public int total { get; set; }
	}

	public class TankoubonsItem
	{
		public Tankoubons result { get; set; } = null!;
		public int filtered { get; set; }
		public int total { get; set; }
	}

	public class ArchiveTankoubons : GenericApiResult
	{
		public List<string> tankoubons { get; set; } = null!;
	}
}
