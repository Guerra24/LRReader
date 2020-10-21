using LRReader.Shared.Internal;
using System.IO;
using System.Threading.Tasks;

namespace LRReader.UWP.Impl
{
	public class FilesStorage : IFilesStorage
	{
		public Task<string> GetFile(string path)
		{
			return File.ReadAllTextAsync(path);
		}

		public Task StoreFile(string path, string content)
		{
			return File.WriteAllTextAsync(path, content);
		}
	}
}
