using Microsoft.Toolkit.Uwp.UI;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.Internal
{
	public class ImageManager
	{
		private StorageFolder localCache = ApplicationData.Current.LocalCacheFolder;
		private StorageFolder thumbnailsFolder;

		public async void Init()
		{
			thumbnailsFolder = await localCache.CreateFolderAsync("Thumbnails", CreationCollisionOption.OpenIfExists);
			await ImageCache.Instance.InitializeAsync(localCache);
			ImageCache.Instance.CacheDuration = TimeSpan.MaxValue;
		}

		public async Task<StorageFile> DownloadThumbnailAsync(string id)
		{
			StorageFile thumbnail = await thumbnailsFolder.CreateFileAsync(id, CreationCollisionOption.OpenIfExists);
			BasicProperties bp = await thumbnail.GetBasicPropertiesAsync();
			if (bp.Size != 0)
				return thumbnail;

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/thumbnail");

			rq.AddParameter("id", id);

			var r = await client.ExecuteGetTaskAsync(rq);

			if (r.StatusCode == HttpStatusCode.OK)
			{
				await FileIO.WriteBytesAsync(thumbnail, r.RawBytes);
				return thumbnail;
			}
			return null;
		}

		public async Task<byte[]> DownloadThumbnailRuntime(string id)
		{
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/thumbnail");

			rq.AddParameter("id", id);

			var r = await client.ExecuteGetTaskAsync(rq);

			if (r.StatusCode == HttpStatusCode.OK)
			{
				return r.RawBytes;
			}
			return null;
		}

		public async Task<BitmapImage> DownloadImage(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;
			if (Global.SettingsManager.ImageCaching)
			{
				return await DownloadImageCache(path);
			}
			else
			{
				var image = new BitmapImage();
				image.UriSource = new Uri(Global.SettingsManager.Profile.ServerAddress + "/" + path);
				return image;
			}
		}

		public async Task<BitmapImage> DownloadImageCache(string path)
		{
			return await ImageCache.Instance.GetFromCacheAsync(new Uri(Global.SettingsManager.Profile.ServerAddress + "/" + path));
		}

		public async Task ClearCache()
		{
			await ImageCache.Instance.ClearAsync();
		}

		public async Task<string> GetCacheSizeMB()
		{
			var imageCache = await localCache.GetFolderAsync("ImageCache");
			var folders = imageCache.CreateFileQuery(CommonFileQuery.OrderByName);

			var fileSizeTasks = (await folders.GetFilesAsync()).Select(async file => (await file.GetBasicPropertiesAsync()).Size);

			var sizes = await Task.WhenAll(fileSizeTasks);

			var folderSize = sizes.Sum(l => (long)l);
			return string.Format("{0:n2} MB", folderSize / 1024f / 1024f);
		}
	}
}
