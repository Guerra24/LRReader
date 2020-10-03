using Caching;
using LRReader.Shared.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Internal
{
	public class ImagesManager
	{

		private LRUCache<string, byte[]> cache;

		public ImagesManager()
		{
			cache = new LRUCache<string, byte[]>(100, 5, false);
		}

		public async Task<byte[]> GetImageCached(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			byte[] data;
			if (cache.TryGet(path, out data))
			{
				return data;
			}
			else
			{
				cache.AddReplace(path, data = await ArchivesProvider.GetImage(path));
				return data;
			}
		}
	}
}
