using Caching;
using KeyedSemaphores;
using LRReader.Shared.Providers;
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
			var key = await KeyedSemaphore.LockAsync(path);
			byte[] data;
			if (cache.TryGet(path, out data))
			{
				key.Dispose();
				return data;
			}
			else
			{
				cache.AddReplace(path, data = await ArchivesProvider.GetImage(path));
				key.Dispose();
				return data;
			}
		}
	}
}
