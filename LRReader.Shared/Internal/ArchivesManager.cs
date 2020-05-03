using static LRReader.Shared.Providers.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;

namespace LRReader.Shared.Internal
{
	public class ArchivesManager
	{
		public List<Archive> Archives = new List<Archive>();

		public async Task ReloadArchives()
		{
			Archives.Clear();
			var result = await ArchivesProvider.LoadArchives();
			if (result != null)
				Archives = result;
		}

	}
}
