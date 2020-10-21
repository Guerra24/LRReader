using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Internal
{
	public interface IFilesStorage
	{
		Task<string> GetFile(string path);

		Task StoreFile(string path, string content);
	}

	public class StubFilesStorage : IFilesStorage
	{
		public async Task<string> GetFile( string path)
		{
			await Task.Delay(1);
			return "";
		}

		public async Task StoreFile(string path, string content)
		{
			await Task.Delay(1);
		}
	}
}
