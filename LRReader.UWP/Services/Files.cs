using System;
using System.IO;
using System.Threading.Tasks;
using LRReader.Shared.Services;
using Windows.Storage;

namespace LRReader.UWP.Services
{
	public class FilesService : IFilesService
	{
		public string LocalCache { get; } = ApplicationData.Current.LocalCacheFolder.Path;

		public string Local { get; } = ApplicationData.Current.LocalFolder.Path;

		public Task<string> GetFile(string path) => File.ReadAllTextAsync(path);

		public Task<byte[]> GetFileBytes(string path) => File.ReadAllBytesAsync(path);

		public Task StoreFile(string path, string content) => File.WriteAllTextAsync(path, content);

		public Task StoreFile(string path, byte[] content) => File.WriteAllBytesAsync(path, content);

		public async Task StoreFileSafe(string path, string content)
		{
			if (!File.Exists(path))
			{
				using (File.Create(path)) { }
			}
			int attempts = 5;
			while (attempts > 0)
			{
				try
				{
					attempts--;
					await PathIO.WriteTextAsync(path, content).AsTask().ConfigureAwait(false);
					break;
				}
				catch (Exception)
				{
					await Task.Delay(100).ConfigureAwait(false);
				}
			}
		}

	}
}
