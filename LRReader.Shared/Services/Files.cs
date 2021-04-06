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

	}

	public class StubFilesService : IFilesService
	{
		public string LocalCache => "";

		public string Local => "";

		public async Task<string> GetFile(string path)
		{
			await Task.Delay(1);
			return "";
		}

		public async Task<byte[]> GetFileBytes(string path)
		{
			await Task.Delay(1);
			return new byte[0];
		}

		public Task StoreFile(string path, string content) => Task.Delay(1);

		public Task StoreFile(string path, byte[] content) => Task.Delay(1);

	}

}
