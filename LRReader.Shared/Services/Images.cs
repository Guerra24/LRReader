using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caching;
using KeyedSemaphores;
using LRReader.Shared.Formats.JpegXL;
using LRReader.Shared.Providers;
using Sentry;

namespace LRReader.Shared.Services;

public class RawImage
{
	public byte[] Data { get; set; } = null!;
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

	//private static readonly string NoThumbHash = "BE17DA00E485ECAFDCB25101EE4FBA34";

	public ImagesService(IFilesService files, ImageProcessingService imageProcessing)
	{
		Files = files;
		ImageProcessing = imageProcessing;
		imagesCache = new LRUCache<string, byte[]>(500, 50);
		imagesSizeCache = new LRUCache<string, Size>(10000, 100);
		thumbnailsCache = new LRUCache<string, byte[]>(5000, 100);
		thumbnailCacheDirectory = Directory.CreateDirectory(Files.LocalCache + "/Images/Thumbnails");
		SixLabors.ImageSharp.Configuration.Default.Configure(new JpegXLConfigurationModule());
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
				files.Where(file => file.CreationTime < DateTime.Now.AddDays(-14) || file.Length == 0).ToList().ForEach(file => file.Delete());
				var directories = thumbnailCacheDirectory.GetDirectories();
				foreach (var dir in directories)
				{
					var archives = dir.GetDirectories();
					archives.Where(dir => dir.GetFiles().Length == 0).ToList().ForEach(dir => dir.Delete());
				}
				directories.Where(dir => dir.GetDirectories().Length == 0).ToList().ForEach(dir => dir.Delete());
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
			}
		});
	}

	public async Task<Size> GetImageSizeCached(string? path)
	{
		if (string.IsNullOrEmpty(path))
			return new Size(0, 0);
		using (var key = await KeyedSemaphore.LockAsync(path + "size").ConfigureAwait(false))
		{
			if (imagesSizeCache.TryGet(path!, out var size))
			{
				return size;
			}
			else
			{
				var image = await GetImageCached(path).ConfigureAwait(false);
				if (image == null)
					return Size.Empty;
				size = await ImageProcessing.GetImageSize(image).ConfigureAwait(false);
				imagesSizeCache.AddReplace(path!, size);
				return size;
			}
		}
	}

	public async Task<byte[]?> GetImageCached(string? path, bool forced = false)
	{
		if (string.IsNullOrEmpty(path))
			return null;
		using (var key = await KeyedSemaphore.LockAsync(path!).ConfigureAwait(false))
		{
			if (imagesCache.TryGet(path!, out var image) && !forced)
			{
				return image;
			}
			else
			{
				image = await ArchivesProvider.GetImage(path!).ConfigureAwait(false);
				if (image == null)
					return null;
				imagesCache.AddReplace(path!, image);
				return image;
			}
		}
	}

	public async Task<byte[]?> GetThumbnailCached(string? id, int page = 0, bool forced = false, bool ignoreCache = false, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(id))
			return null;
		var thumbKey = $"{id}.{page}";
		if (ignoreCache)
			return await GetThumbnailRaw(id!, page, cancellationToken).ConfigureAwait(false);
		using (var key = await KeyedSemaphore.LockAsync(thumbKey).ConfigureAwait(false))
		{
			if (cancellationToken.IsCancellationRequested)
				return null;
			if (thumbnailsCache.TryGet(thumbKey, out var data) && !forced)
			{
				return data;
			}
			else
			{
				var path = $"{thumbnailCacheDirectory.FullName}/{id!.Substring(0, 2)}/{id}/{page}.cache";
				if (await Task.Run(() => File.Exists(path)).ConfigureAwait(false) && !forced)
				{
					if (cancellationToken.IsCancellationRequested)
						return null;
					data = await Files.GetFileBytes(path).ConfigureAwait(false);
					/*if (data.Length == 55876)
						using (var md5 = System.Security.Cryptography.MD5.Create())
							if (NoThumbHash.Equals(string.Concat(md5.ComputeHash(data).Select(x => x.ToString("X2")))))
							{
								File.Delete(path);
								data = await GetThumbnailRaw(id, page);
								if (data == null)
									return null;
								Directory.CreateDirectory(Path.GetDirectoryName(path));
								await Files.StoreFile(path, data);
							}*/
				}
				else
				{
					data = await GetThumbnailRaw(id, page, cancellationToken).ConfigureAwait(false);
					if (data == null || data.Length == 0)
						return null;
					if (cancellationToken.IsCancellationRequested)
						return null;
					// While overloaded the api might return ok but without any content
					//if (data.Length == 55876)
					//	using (var md5 = System.Security.Cryptography.MD5.Create())
					//		if (NoThumbHash.Equals(string.Concat(md5.ComputeHash(data).Select(x => x.ToString("X2")))))
					//			return null;
					await Task.Run(() => Directory.CreateDirectory(Path.GetDirectoryName(path)!)).ConfigureAwait(false);
					await Files.StoreFile(path, data).ConfigureAwait(false);
				}
				thumbnailsCache.AddReplace(thumbKey, data);
				return data;
			}
		}
	}

	private async Task<byte[]?> GetThumbnailRaw(string id, int page = 0, CancellationToken cancellationToken = default)
	{
		int retries = 20;
		while (retries >= 0)
		{
			retries--;
			if (cancellationToken.IsCancellationRequested)
				return null;
			var data = await ArchivesProvider.GetThumbnail(id, true, page).ConfigureAwait(false);
			if (data == null)
				return null;
			if (data.Thumbnail != null)
				return data.Thumbnail;
			if (data.Job != null && !await data.Job.WaitForMinionJob(cancellationToken).ConfigureAwait(false))
				break;
		}
		return null;
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
				size += file.Length;
			return string.Format("{0:n2} MB", size / 1024f / 1024f);
		});
	}
}
