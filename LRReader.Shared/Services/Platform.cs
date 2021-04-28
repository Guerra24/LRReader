using System;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{

	public enum Symbols
	{
		Favorite, Pictures
	}

	public interface IPlatformService
	{
		void Init();

		Version GetVersion();

		Task<bool> OpenInBrowser(Uri uri);

		object GetSymbol(Symbols symbol);

	}

	public class StubPlatformService : IPlatformService
	{

		public void Init()
		{
		}

		public Version GetVersion()
		{
			return new Version(0, 0, 0, 0);
		}

		public Task<bool> OpenInBrowser(Uri uri)
		{
			return Task.Run(() => false);
		}

		public object GetSymbol(Symbols symbol)
		{
			return null;
		}
	}
}
