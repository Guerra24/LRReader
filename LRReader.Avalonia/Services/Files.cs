using LRReader.Shared.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{
	public class FilesService : IFilesService
	{
		private readonly string LocalCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LRReader");
		private readonly string LocalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LRReader");

		public string LocalCache => LocalCachePath;

		public string Local => LocalPath;

		public FilesService()
		{
			Directory.CreateDirectory(LocalCachePath);
			Directory.CreateDirectory(LocalPath);
		}

		public Task<string> GetFile(string path) => File.ReadAllTextAsync(path);

		public Task<byte[]> GetFileBytes(string path) => File.ReadAllBytesAsync(path);

		public Task StoreFile(string path, string content) => File.WriteAllTextAsync(path, content);

		public Task StoreFile(string path, byte[] content) => File.WriteAllBytesAsync(path, content);
	}
}
