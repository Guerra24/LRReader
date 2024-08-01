using System.Collections.Generic;

namespace LRReader.Shared.Models.Main
{
	public class Tankoubon
	{
		public List<string> archives { get; set; } = null!;
		public string id { get; set; } = null!;
		public string name { get; set; } = null!;
		public List<Archive>? full_data { get; set; }

		public override string ToString()
		{
			return name;
		}

		public override bool Equals(object? obj)
		{
			if (obj is AddNewTankoubon)
				return false;
			return obj is Tankoubon tankoubon && id == tankoubon.id;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}

	public class TankoubonsList
	{
		public List<Tankoubon> result { get; set; } = null!;
		public int filtered { get; set; }
		public int total { get; set; }
	}

	public class TankoubonsItem
	{
		public Tankoubon result { get; set; } = null!;
		public int filtered { get; set; }
		public int total { get; set; }
	}

	public class ArchiveTankoubons : GenericApiResult
	{
		public List<string> tankoubons { get; set; } = null!;
	}

	public class AddNewTankoubon : Tankoubon
	{
		public override bool Equals(object? obj)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

	}

}
