using Caching;
using KeyedSemaphores;
using LRReader.Shared.Extensions;
using LRReader.Shared.Formats.JpegXL;
using LRReader.Shared.Providers;
using Sentry;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.Services;

public class ImagesService : IService
{
	private readonly IFilesService Files;
	private readonly ImageProcessingService ImageProcessing;

	// Implement persistence
	// https://github.com/jchristn/Caching?tab=readme-ov-file#persistence
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
		thumbnailsCache = new LRUCache<string, byte[]>(5000, 100);
		thumbnailCacheDirectory = Directory.CreateDirectory(Files.LocalCache + "/Images/Thumbnails");
		SixLabors.ImageSharp.Configuration.Default.Configure(new JpegXLConfigurationModule());
	}

	public async Task Init()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
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
	}

	public async Task<Size> GetImageSizeCached(string? path)
	{
		if (string.IsNullOrEmpty(path))
			return new Size(0, 0);
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		if (imagesSizeCache.TryGet(path, out var size))
		{
			return size;
		}
		else
		{
			using (var key = await KeyedSemaphore.LockAsync(path + "size"))
			{
				var image = await GetImageCached(path);
				if (image == null)
					return Size.Empty;
				size = await ImageProcessing.GetImageSize(image);
				imagesSizeCache.AddReplace(path!, size);
				return size;
			}
		}
	}

	public async Task<byte[]?> GetImageCached(string? path, bool forced = false)
	{
		if (string.IsNullOrEmpty(path))
			return null;
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		if (imagesCache.TryGet(path, out var image) && !forced)
		{
			return image;
		}
		else
		{
			using (var key = await KeyedSemaphore.LockAsync(path!))
			{
				image = await ArchivesProvider.GetImage(path!);
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
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		var thumbKey = $"{id}.{page}";
		if (ignoreCache)
			return await GetThumbnailRaw(id!, page, cancellationToken);
		if (cancellationToken.IsCancellationRequested)
			return null;
		if (thumbnailsCache.TryGet(thumbKey, out var data) && !forced)
		{
			return data;
		}
		else
		{
			using (var key = await KeyedSemaphore.LockAsync(thumbKey))
			{
				if (cancellationToken.IsCancellationRequested)
					return null;
				var path = $"{thumbnailCacheDirectory.FullName}/{id!.Substring(0, 2)}/{id}/{page}.cache";
				if (File.Exists(path) && !forced)
				{
					if (cancellationToken.IsCancellationRequested)
						return null;
					data = await Files.GetFileBytes(path);
				}
				else
				{
					data = await GetThumbnailRaw(id, page, cancellationToken);
					if (data == null || data.Length == 0)
						return null;
					if (cancellationToken.IsCancellationRequested)
						return null;
					// While overloaded the api might return ok but without any content
					Directory.CreateDirectory(Path.GetDirectoryName(path)!);
					await Files.StoreFile(path, data);
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
			var data = await ArchivesProvider.GetThumbnail(id, true, page);
			if (data == null)
				return null;
			if (data.Thumbnail != null)
				return data.Thumbnail;
			if (data.Job != null && !await data.Job.WaitForMinionJob(cancellationToken))
				break;
		}
		return null;
	}

	public async Task DeleteThumbnailCache()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

		imagesCache.Clear();
		imagesSizeCache.Clear();
		thumbnailsCache.Clear();
		thumbnailCacheDirectory.GetDirectories().ToList().ForEach(dir => dir.Delete(true));
	}

	public async Task<long> GetThumbnailCacheSize()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		var files = thumbnailCacheDirectory.GetFiles("*.*", SearchOption.AllDirectories);
		return files.Sum(f => f.Length);
	}
}
