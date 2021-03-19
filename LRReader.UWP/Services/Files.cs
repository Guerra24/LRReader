using LRReader.Shared.Services;
using System.IO;
using System.Threading.Tasks;

namespace LRReader.UWP.Services
{
	public class FilesService : IFilesService
	{

		public Task<string> GetFile(string path) => File.ReadAllTextAsync(path);

		public Task StoreFile(string path, string content) => File.WriteAllTextAsync(path, content);

		public void DeleteFile(string path) => File.Delete(path);

		public bool ExistFile(string path) => File.Exists(path);

	}
}
