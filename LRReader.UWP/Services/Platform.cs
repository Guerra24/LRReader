using LRReader.Shared.Services;
using LRReader.UWP.Views;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Tabs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace LRReader.UWP.Services
{
	public class UWPlatformService : PlatformService
	{
		private readonly TabsService Tabs;

		private readonly Uri checkUri = new Uri("check:check");

		private readonly Dictionary<Symbols, SymbolIconSource> SymbolToSymbol = new Dictionary<Symbols, SymbolIconSource>();

		private UISettings UISettings;

		private Root Root;

		public UWPlatformService(TabsService tabs, ILoggerFactory loggerFactory, IFilesService files)
		{
			Tabs = tabs;
#if DEBUG
			loggerFactory.AddFile(files.LocalCache + string.Format("/Logs/App-{0:yyyy}-{0:MM}-{0:dd}.log", DateTime.UtcNow));
#endif
		}

		public override void Init()
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

			MapDialogToType(Dialog.CategoryArchive, typeof(CategoryArchive));

			SymbolToSymbol.Add(Symbols.Favorite, new SymbolIconSource { Symbol = Symbol.Favorite });
			SymbolToSymbol.Add(Symbols.Pictures, new SymbolIconSource { Symbol = Symbol.Pictures });

			UISettings = new UISettings();

			Root = Window.Current.Content as Root;
		}

		public override Version Version
		{
			get
			{
				var version = Package.Current.Id.Version;
				return new Version(version.Major, version.Minor, version.Build, version.Revision);
			}
		}

		public override bool AnimationsEnabled => UISettings.AnimationsEnabled;

		public override uint HoverTime => UISettings.MouseHoverTime;

		public override Task<bool> OpenInBrowser(Uri uri) => Launcher.LaunchUriAsync(uri).AsTask();

		public override object GetSymbol(Symbols symbol)
		{
			SymbolIconSource symb;
			if (!SymbolToSymbol.TryGetValue(symbol, out symb))
				return null;
			return symb;
		}

		public override string GetLocalizedString(string key)
		{
			var split = key.Split(new[] { '/' }, 2);
			return ResourceLoader.GetForCurrentView(split[0]).GetString(split[1]);
		}

		public string GetPackageFamilyName() => Package.Current.Id.FamilyName;

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

		public override void ChangeTheme(AppTheme theme) => Root.ChangeTheme(theme);
	}
}
