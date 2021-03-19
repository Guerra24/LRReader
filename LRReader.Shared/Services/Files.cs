using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public interface IFilesService
	{
		Task<string> GetFile(string path);

		Task StoreFile(string path, string content);

		void DeleteFile(string path);

		bool ExistFile(string path);
	}

	public class StubFilesService : IFilesService
	{
		public async Task<string> GetFile(string path)
		{
			await Task.Delay(1);
			return "";
		}

		public async Task StoreFile(string path, string content)
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
