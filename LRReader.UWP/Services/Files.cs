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

		public async Task<string> GetFile(string path) => await File.ReadAllTextAsync(path).ConfigureAwait(false);

		public async Task<byte[]> GetFileBytes(string path) => await File.ReadAllBytesAsync(path).ConfigureAwait(false);

		public async Task StoreFile(string path, string content) => await File.WriteAllTextAsync(path, content).ConfigureAwait(false);

		public async Task StoreFile(string path, byte[] content) => await File.WriteAllBytesAsync(path, content).ConfigureAwait(false);

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
