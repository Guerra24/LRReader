using LRReader.Avalonia.Views.Tabs;
using LRReader.Shared.Services;
using System;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{
	class AvaloniaPlatformService : IPlatformService
	{
		private readonly TabsService Tabs;

		public AvaloniaPlatformService(TabsService tabs)
		{
			Tabs = tabs;
		}

		public void Init()
		{
			Tabs.MapTabToType(Tab.Archives, typeof(ArchivesTab));
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
