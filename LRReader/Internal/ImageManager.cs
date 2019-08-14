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
			IStorageItem thumb = await thumbnailsFolder.TryGetItemAsync(id);
			if (thumb != null)
			{
				StorageFile tmp = await thumbnailsFolder.GetFileAsync(id);
				int i = 0;
				while (i < 20)
				{
					i++;
					BasicProperties bp = await tmp.GetBasicPropertiesAsync();
					if (bp.Size != 0)
						return tmp;
					await Task.Delay(200);
				}
			}

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/thumbnail");

			rq.AddParameter("id", id);

			var r = await client.ExecuteGetTaskAsync(rq);

			if (r.StatusCode == HttpStatusCode.OK)
			{
				StorageFile thumbnail = await thumbnailsFolder.CreateFileAsync(id, CreationCollisionOption.ReplaceExisting);
				await FileIO.WriteBytesAsync(thumbnail, r.RawBytes);
				return thumbnail;
			}

			return null;
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
