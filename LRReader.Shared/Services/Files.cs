using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public interface IFilesService
	{
		string LocalCache { get; }

		string Local { get; }

		Task<string> GetFile(string path);

		Task<byte[]> GetFileBytes(string path);

		Task StoreFile(string path, string content);

		Task StoreFile(string path, byte[] content);

		Task StoreFileSafe(string path, string content);

	}

	public class StubFilesService : IFilesService
	{
		public string LocalCache => "";

		public string Local => "";

		public Task<string> GetFile(string path) => Task.FromResult("");

		public Task<byte[]> GetFileBytes(string path) => Task.FromResult(new byte[0]);

		public Task StoreFile(string path, string content) => Task.CompletedTask;

		public Task StoreFile(string path, byte[] content) => Task.CompletedTask;

		public Task StoreFileSafe(string path, string content) => Task.CompletedTask;

	}

}
