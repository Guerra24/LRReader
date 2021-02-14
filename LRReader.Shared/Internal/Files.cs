using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Internal
{
	public interface IFilesStorage
	{
		Task<string> GetFile(string path);

		Task StoreFile(string path, string content);

		void DeleteFile(string path);

		bool ExistFile(string path);
	}

	public class StubFilesStorage : IFilesStorage
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

	public static class FilesStorage
	{
		private static IFilesStorage provider = new StubFilesStorage();

		public static void SetProvider(IFilesStorage filesStorage) => provider = filesStorage;

		public static Task<string> GetFile(string path) => provider.GetFile(path);

		public static Task StoreFile(string path, string content) => provider.StoreFile(path, content);

		public static void DeleteFile(string path) => provider.DeleteFile(path);

		public static bool ExistFile(string path) => provider.ExistFile(path);
	}

}
