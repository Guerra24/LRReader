using Caching;
using KeyedSemaphores;
using LRReader.Shared.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ImagesService
	{
		private readonly IFilesService Files;
		private LRUCache<string, byte[]> archivesCache;
		private LRUCache<string, byte[]> thumbnailsCache;
		//private string thumbnailCacheDirectory;

		public ImagesService(IFilesService files)
		{
			Files = files;
			archivesCache = new LRUCache<string, byte[]>(100, 10);
			thumbnailsCache = new LRUCache<string, byte[]>(500, 50);
			//thumbnailCacheDirectory = Files.CreateDirectory(Files.LocalCache + "/Thumbnails").FullName;
		}

		public async Task<byte[]> GetImageCached(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			var key = await KeyedSemaphore.LockAsync(path);
			byte[] data;
			if (archivesCache.TryGet(path, out data))
			{
				key.Dispose();
				return data;
			}
			else
			{
				archivesCache.AddReplace(path, data = await ArchivesProvider.GetImage(path));
				key.Dispose();
				return data;
			}
		}

		public async Task<byte[]> GetThumbnailCached(string id)
		{
			if (string.IsNullOrEmpty(id))
				return null;
			var key = await KeyedSemaphore.LockAsync(id);
			byte[] data;
			if (thumbnailsCache.TryGet(id, out data)/* && !forced*/)
			{
				key.Dispose();
				return data;
			}
			else
			{
				/*var directory = $"{thumbnailCacheDirectory}/{id.Substring(0,2)}/";
				var path = $"{directory}{id}.cache";
				if (Files.ExistFile(path) && !forced)
					data = await Files.GetFileBytes(path);
				else
				{*/
					data = await ArchivesProvider.GetThumbnail(id);
					/*Files.CreateDirectory(directory);
					await Files.StoreFile(path, data);
				}*/
				thumbnailsCache.AddReplace(id, data);
				key.Dispose();
				return data;
			}
		}
	}
}
