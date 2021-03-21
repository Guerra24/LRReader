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

		void DeleteFile(string path);

		bool ExistFile(string path);

	}

	public class StubFilesService : IFilesService
	{
		public string LocalCache => throw new System.NotImplementedException();

		public string Local => throw new System.NotImplementedException();

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

		public async Task StoreFile(string path, string content)
		{
			await Task.Delay(1);
		}

		public async Task StoreFile(string path, byte[] content)
		{
			await Task.Delay(1);
		}

		public void DeleteFile(string path)
		{
		}

		public bool ExistFile(string path)
		{
			return false;
		}


	}

}
