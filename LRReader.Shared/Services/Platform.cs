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

		Version Version { get; }

		bool AnimationsEnabled { get; }

		uint HoverTime { get; }

		Task<bool> OpenInBrowser(Uri uri);

		object GetSymbol(Symbols symbol);

		string GetLocalizedString(string key);

	}

	public class StubPlatformService : IPlatformService
	{

		public void Init()
		{
		}

		public Version Version => new Version(0, 0, 0, 0);

		public bool AnimationsEnabled => false;

		public uint HoverTime => 300;

		public Task<bool> OpenInBrowser(Uri uri)
		{
			return Task.Run(() => false);
		}

		public object GetSymbol(Symbols symbol)
		{
			return null;
		}

		public string GetLocalizedString(string key)
		{
			return key;
		}
	}
}
