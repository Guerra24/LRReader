using Caching;
using KeyedSemaphores;
using LRReader.Shared.Providers;
using Microsoft.AppCenter.Crashes;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class RawImage
	{
		public byte[] Data { get; set; }
		public Size Size { get; set; }
	}

	public class ImagesService : IService
	{
		private readonly IFilesService Files;
		private readonly ImageProcessingService ImageProcessing;

		private LRUCache<string, byte[]> imagesCache;
		private LRUCache<string, Size> imagesSizeCache;
		private LRUCache<string, byte[]> thumbnailsCache;
		private DirectoryInfo thumbnailCacheDirectory;

		public ImagesService(IFilesService files, ImageProcessingService imageProcessing)
		{
			Files = files;
			ImageProcessing = imageProcessing;
			imagesCache = new LRUCache<string, byte[]>(500, 50);
			imagesSizeCache = new LRUCache<string, Size>(10000, 100);
			thumbnailsCache = new LRUCache<string, byte[]>(1000, 50);
			Directory.CreateDirectory(Files.LocalCache + "/Images");
			thumbnailCacheDirectory = Directory.CreateDirectory(Files.LocalCache + "/Images/Thumbnails");
		}

		public Task Init()
		{
			return Task.Run(() =>
			{
				try
				{
					imagesCache.Clear();
					imagesSizeCache.Clear();
					thumbnailsCache.Clear();
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

		public async Task<Size> GetImageSizeCached(string path)
		{
			if (string.IsNullOrEmpty(path))
				return new Size(0, 0);
			using (var key = await KeyedSemaphore.LockAsync(path + "size"))
			{
				Size size;
				if (imagesSizeCache.TryGet(path, out size))
				{
					return size;
				}
				else
				{
					var image = await GetImageCached(path);
					if (image == null)
						return new Size(0, 0);
					size = await ImageProcessing.GetImageSize(image);
					imagesSizeCache.AddReplace(path, size);
					return size;
				}
			}
		}

		public async Task<byte[]> GetImageCached(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			using (var key = await KeyedSemaphore.LockAsync(path))
			{
				byte[] image;
				if (imagesCache.TryGet(path, out image))
				{
					return image;
				}
				else
				{
					image = await ArchivesProvider.GetImage(path);
					if (image == null)
						return null;
					imagesCache.AddReplace(path, image);
					return image;
				}
			}
		}

		public async Task<byte[]> GetThumbnailCached(string id, bool forced = false, bool ignoreCache = false)
		{
			if (string.IsNullOrEmpty(id))
				return null;
			if (ignoreCache)
				return await ArchivesProvider.GetThumbnail(id);
			using (var key = await KeyedSemaphore.LockAsync(id))
			{
				byte[] data;
				if (thumbnailsCache.TryGet(id, out data) && !forced)
				{
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
						if (data == null)
							return null;
						Directory.CreateDirectory(directory);
						await Files.StoreFile(path, data);
					}
					thumbnailsCache.AddReplace(id, data);
					return data;
				}
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
