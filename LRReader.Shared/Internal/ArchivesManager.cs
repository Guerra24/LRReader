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
		public List<TagStats> TagStats = new List<TagStats>();

		public async Task ReloadArchives()
		{
			Archives.Clear();
			var resultA = await ArchivesProvider.LoadArchives();
			if (resultA != null)
				Archives = resultA;
			TagStats.Clear();
			var resultT = await ArchivesProvider.LoadTagStats();
			if (resultT != null)
			{
				await Task.Run(() =>
				{
					foreach (var t in resultT)
						TagStats.Add(t);
				});
			}
		}

	}
}
