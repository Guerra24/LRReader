using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace LRReader.Internal
{
	public class ImageManager
	{

		private StorageFolder thumbnailsFolder;

		public ImageManager()
		{
		}

		public async void Init()
		{
			StorageFolder localCache = ApplicationData.Current.LocalCacheFolder;
			thumbnailsFolder = await localCache.CreateFolderAsync("thumbnails", CreationCollisionOption.OpenIfExists);
		}

		public async Task<StorageFile> DownloadThumbnailAsync(string id)
		{
			IStorageItem thumb = await thumbnailsFolder.TryGetItemAsync(id);
			if (thumb != null)
			{
				StorageFile tmp = await thumbnailsFolder.GetFileAsync(id);
				BasicProperties bp = await tmp.GetBasicPropertiesAsync();
				if (bp.Size != 0)
					return tmp;
			}

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/thumbnail");

			rq.AddParameter("id", id);

			StorageFile thumbnail = await thumbnailsFolder.CreateFileAsync(id);

			var r = await client.ExecuteGetTaskAsync(rq);

			if (r.StatusCode == HttpStatusCode.OK)
			{
				await FileIO.WriteBytesAsync(thumbnail, r.RawBytes);
				return thumbnail;
			}

			return null;
		}
	}
}
