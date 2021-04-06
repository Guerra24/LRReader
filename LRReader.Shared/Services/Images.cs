using Caching;
using KeyedSemaphores;
using LRReader.Shared.Providers;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ImagesService : IService
	{
		private readonly IFilesService Files;
		private LRUCache<string, byte[]> archivesCache;
		private LRUCache<string, byte[]> thumbnailsCache;
		private DirectoryInfo thumbnailCacheDirectory;

		public ImagesService(IFilesService files)
		{
			Files = files;
			archivesCache = new LRUCache<string, byte[]>(100, 10);
			thumbnailsCache = new LRUCache<string, byte[]>(500, 50);
			Directory.CreateDirectory(Files.LocalCache + "/Images");
			thumbnailCacheDirectory = Directory.CreateDirectory(Files.LocalCache + "/Images/Thumbnails");
		}

		public Task Init()
		{
			return Task.Run(() =>
			{
				try
				{
					var files = thumbnailCacheDirectory.GetFiles("*.*", SearchOption.AllDirectories);
					files.Where(file => file.CreationTime < DateTime.Now.AddDays(-14)).ToList().ForEach(file => file.Delete());
					var directories = thumbnailCacheDirectory.GetDirectories();
					directories.Where(dir => dir.GetFiles().Length == 0).ToList().ForEach(dir => dir.Delete());
				}
				catch (Exception e)
				{
					Crashes.TrackError(e);
				}
			});
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

		public async Task<byte[]> GetThumbnailCached(string id, bool forced = false)
		{
			if (string.IsNullOrEmpty(id))
				return null;
			var key = await KeyedSemaphore.LockAsync(id);
			byte[] data;
			if (thumbnailsCache.TryGet(id, out data) && !forced)
			{
				key.Dispose();
				return data;
			}
			else
			{
				var directory = $"{thumbnailCacheDirectory.FullName}/{id.Substring(0, 2)}/";
				var path = $"{directory}{id}.cache";
				if (File.Exists(path) && !forced)
					data = await Files.GetFileBytes(path);
				else
				{
					data = await ArchivesProvider.GetThumbnail(id);
					Directory.CreateDirectory(directory);
					await Files.StoreFile(path, data);
				}
				thumbnailsCache.AddReplace(id, data);
				key.Dispose();
				return data;
			}
		}

		public Task DeleteThumbnailCache()
		{
			return Task.Run(() => thumbnailCacheDirectory.GetDirectories().ToList().ForEach(dir => dir.Delete(true)));
		}

		public Task<string> GetThumbnailCacheSize()
		{
			return Task.Run(() =>
			{
				var files = thumbnailCacheDirectory.GetFiles("*.*", SearchOption.AllDirectories);
				long size = 0;
				foreach (var file in files)
				{
					size += file.Length;
				}
				return string.Format("{0:n2} MB", size / 1024f / 1024f);
			});
		}
	}
}
