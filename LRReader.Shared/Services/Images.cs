using Caching;
using KeyedSemaphores;
using LRReader.Shared.Extensions;
using LRReader.Shared.Formats.LibAvif;
using LRReader.Shared.Formats.LibJpegXL;
using LRReader.Shared.Providers;
using System.Drawing;

namespace LRReader.Shared.Services;

public class ImagesService : IService
{
	private readonly ImageProcessingService ImageProcessing;

	private readonly KeyedSemaphoresDictionary<string> keyedSemaphores = new(Environment.ProcessorCount, 1024, EqualityComparer<string>.Default, TimeSpan.FromMilliseconds(10));

	// Implement persistence
	// https://github.com/jchristn/Caching?tab=readme-ov-file#persistence
	private readonly LRUCache<string, byte[]> imagesCache;
	private readonly LRUCache<string, Size> imagesSizeCache;
	private readonly LRUCache<string, byte[]> thumbnailsCache;
	private readonly DirectoryInfo thumbnailCacheDirectory;

	public ImagesService(IFilesService files, ImageProcessingService imageProcessing)
	{
		ImageProcessing = imageProcessing;
		imagesCache = new LRUCache<string, byte[]>(1000, 25)
		{
			SlidingExpiration = true,
			MaxMemoryBytes = 536870912 // 512MiB
		};
		imagesCache.SizeEstimator += SizeEstimator;
		imagesSizeCache = new LRUCache<string, Size>(10000, 100);
		thumbnailsCache = new LRUCache<string, byte[]>(5000, 100)
		{
			SlidingExpiration = true,
			MaxMemoryBytes = 134217728 // 128MiB
		};
		thumbnailsCache.SizeEstimator += SizeEstimator;
		thumbnailCacheDirectory = Directory.CreateDirectory(Path.Combine(files.LocalCache, "Images", "Thumbnails"));
		SixLabors.ImageSharp.Configuration.Default.Configure(new JpegXLConfigurationModule());
		SixLabors.ImageSharp.Configuration.Default.Configure(new AvifConfigurationModule());
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

	public async Task<Size> GetImageSizeCached(string? path, CancellationToken cancellationToken = default)
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
			try
			{
				using var key = await keyedSemaphores.LockAsync(path + "size", cancellationToken);
				var image = await GetImageCached(path, cancellationToken: cancellationToken);
				if (image == null)
					return Size.Empty;
				if (cancellationToken.IsCancellationRequested)
					return Size.Empty;
				size = await ImageProcessing.GetImageSize(image);
				await imagesSizeCache.AddReplaceAsync(path, size);
				return size;
			}
			catch (OperationCanceledException)
			{
				return Size.Empty;
			}
		}
	}

	public async Task<byte[]?> GetImageCached(string? path, bool forced = false, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(path))
			return null;
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		if (cancellationToken.IsCancellationRequested)
			return null;
		if (imagesCache.TryGet(path, out var image) && !forced)
		{
			return image;
		}
		else
		{
			try
			{
				using var key = await keyedSemaphores.LockAsync(path, cancellationToken);
				if (cancellationToken.IsCancellationRequested)
					return null;
				image = await ArchivesProvider.GetImage(path, cancellationToken);
				if (image == null)
					return null;
				if (cancellationToken.IsCancellationRequested)
					return null;
				await imagesCache.AddReplaceAsync(path, image, DateTime.UtcNow.AddMinutes(15));
				return image;
			}
			catch (OperationCanceledException)
			{
				return null;
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
			try
			{
				using var key = await keyedSemaphores.LockAsync(thumbKey, cancellationToken);
				if (cancellationToken.IsCancellationRequested)
					return null;
				var path = Path.Combine(thumbnailCacheDirectory.FullName, id[..2], id, $"{page}.cache");
				if (File.Exists(path) && !forced)
				{
					if (cancellationToken.IsCancellationRequested)
						return null;
					data = await File.ReadAllBytesAsync(path, cancellationToken);
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
					await File.WriteAllBytesAsync(path, data, cancellationToken);
				}
				await thumbnailsCache.AddReplaceAsync(thumbKey, data, DateTime.UtcNow.AddMinutes(15));
				return data;
			}
			catch (OperationCanceledException)
			{
				return null;
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
			var data = await ArchivesProvider.GetThumbnail(id, page, cancellationToken);
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

		await imagesCache.ClearAsync();
		await imagesSizeCache.ClearAsync();
		await thumbnailsCache.ClearAsync();
		thumbnailCacheDirectory.GetDirectories().ToList().ForEach(dir => dir.Delete(true));
	}

	public async Task<long> GetThumbnailCacheSize()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		var files = thumbnailCacheDirectory.GetFiles("*.*", SearchOption.AllDirectories);
		return files.Sum(f => f.Length);
	}

	private long SizeEstimator(byte[] data) => data.Length;
}
