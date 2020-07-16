using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;

namespace LRReader.Shared.Internal
{
	public class ArchivesManager
	{
		public List<Archive> Archives = new List<Archive>();
		public List<TagStats> TagStats = new List<TagStats>();

		public async Task ReloadArchives()
		{
			Archives.Clear();
			var resultA = await ArchivesProvider.GetArchives();
			if (resultA != null)
				Archives = resultA;
			TagStats.Clear();
			var resultT = await DatabaseProvider.GetTagStats();
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
