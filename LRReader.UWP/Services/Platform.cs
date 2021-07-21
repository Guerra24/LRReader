using LRReader.Shared.Services;
using LRReader.UWP.Views.Tabs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace LRReader.UWP.Services
{
	public class UWPlatformService : IPlatformService
	{
		private readonly TabsService Tabs;

		private readonly Uri checkUri = new Uri("check:check");

		private readonly Dictionary<Symbols, SymbolIconSource> SymbolToSymbol = new Dictionary<Symbols, SymbolIconSource>();

		private UISettings UISettings;

		public UWPlatformService(TabsService tabs)
		{
			Tabs = tabs;
		}

		public void Init()
		{
			Tabs.MapTabToType(Tab.Archives, typeof(ArchivesTab));
			Tabs.MapTabToType(Tab.Archive, typeof(ArchiveTab));
			Tabs.MapTabToType(Tab.ArchiveEdit, typeof(ArchiveEditTab));
			Tabs.MapTabToType(Tab.Bookmarks, typeof(BookmarksTab));
			Tabs.MapTabToType(Tab.Categories, typeof(CategoriesTab));
			Tabs.MapTabToType(Tab.CategoryEdit, typeof(CategoryEditTab));
			Tabs.MapTabToType(Tab.SearchResults, typeof(SearchResultsTab));
			Tabs.MapTabToType(Tab.Settings, typeof(SettingsTab));
			Tabs.MapTabToType(Tab.Web, typeof(WebTab));
			Tabs.MapTabToType(Tab.Tools, typeof(ToolsTab));

			SymbolToSymbol.Add(Symbols.Favorite, new SymbolIconSource { Symbol = Symbol.Favorite });
			SymbolToSymbol.Add(Symbols.Pictures, new SymbolIconSource { Symbol = Symbol.Pictures });

			UISettings = new UISettings();
		}

		public Version Version
		{
			get
			{
				var version = Package.Current.Id.Version;
				return new Version(version.Major, version.Minor, version.Build, version.Revision);
			}
		}

		public bool AnimationsEnabled => UISettings.AnimationsEnabled;

		public uint HoverTime => UISettings.MouseHoverTime;

		public Task<bool> OpenInBrowser(Uri uri)
		{
			return Launcher.LaunchUriAsync(uri).AsTask();
		}

		public object GetSymbol(Symbols symbol)
		{
			SymbolIconSource symb;
			if (!SymbolToSymbol.TryGetValue(symbol, out symb))
				return null;
			return symb;
		}

		public string GetPackageFamilyName()
		{
			return Package.Current.Id.FamilyName;
		}

		public async Task<bool> CheckAppInstalled(string package)
		{
			try
			{
				var result = await Launcher.QueryUriSupportAsync(checkUri, LaunchQuerySupportType.Uri, package);
				switch (result)
				{
					case LaunchQuerySupportStatus.Available:
					case LaunchQuerySupportStatus.NotSupported:
						return true;
					default:
						return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
