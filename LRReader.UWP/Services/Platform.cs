using LRReader.Shared.Models;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using LRReader.UWP.Views;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Main;
using LRReader.UWP.Views.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Symbol = LRReader.Shared.Services.Symbol;
using SymbolIconSource = Microsoft.UI.Xaml.Controls.SymbolIconSource;

namespace LRReader.UWP.Services
{
	public class UWPlatformService : PlatformService
	{
		private readonly TabsService Tabs;

		private readonly Uri checkUri = new Uri("check:check");

		private UISettings UISettings = new UISettings();

		private bool _animationsEnabled, _dualScreen;

		private double _dualScreenWidth;

		private Root Root = null!;

		private Dictionary<string, string> LocalizationCache = new();

		public UWPlatformService(TabsService tabs, IFilesService files)
		{
			Tabs = tabs;
			_animationsEnabled = UISettings.AnimationsEnabled;

#pragma warning disable CA1416 // Validate platform compatibility
			if (WinRT_IsApiContractPresent("Windows.Foundation.UniversalApiContract", 10))
				UISettings.AnimationsEnabledChanged += (sender, args) => _animationsEnabled = sender.AnimationsEnabled;

			if (WinRT_IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
			{
				var device = new EasClientDeviceInformation();

				if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Core" || (!string.IsNullOrEmpty(device.SystemSku) && device.SystemSku.Contains("Surface_Duo")))
				{
					var winEnv = WindowingEnvironment.FindAll(WindowingEnvironmentKind.Overlapped).FirstOrDefault();
					if (winEnv != null)
					{
						var regions = winEnv.GetDisplayRegions();
						if (regions.Count == 2 && regions[0].WorkAreaSize.Width == regions[1].WorkAreaSize.Width)
						{
							_dualScreenWidth = regions[0].WorkAreaSize.Width + regions[1].WorkAreaSize.Width; // WCOS reports this in virtual size
							if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
							{
								// Desktop returns regions as raw pixels so convert them to virtual size
								var dpi = DisplayInformation.GetForCurrentView();
								_dualScreenWidth /= dpi.RawPixelsPerViewPixel;
								//Debug.WriteLine(_dualScreenWidth);
								// dpi.ScreenWidthInRawPixels on WCOS returns the sum of width of both displays also in virtual size
								//Debug.WriteLine(dpi.ScreenWidthInRawPixels * 2 / dpi.RawPixelsPerViewPixel);
							}
							_dualScreen = true;
						}
					}
				}
			}
#pragma warning restore CA1416 // Validate platform compatibility

			Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "00FFFFFF");

			MapPageToType<LoadingPage>(Pages.Loading);
			MapPageToType<FirstRunPage>(Pages.FirstRun);
			MapPageToType<HostTabPage>(Pages.HostTab);

			MapTransitionToType<SuppressNavigationTransitionInfo>(PagesTransition.None);
			MapTransitionToType<DrillInNavigationTransitionInfo>(PagesTransition.DrillIn);
		}

		public override void Init()
		{
			Tabs.MapTabToType<ArchivesTab>(Tab.Archives);
			Tabs.MapTabToType<ArchiveTab>(Tab.Archive);
			Tabs.MapTabToType<ArchiveEditTab>(Tab.ArchiveEdit);
			Tabs.MapTabToType<BookmarksTab>(Tab.Bookmarks);
			Tabs.MapTabToType<CategoriesTab>(Tab.Categories);
			Tabs.MapTabToType<CategoryEditTab>(Tab.CategoryEdit);
			Tabs.MapTabToType<SearchResultsTab>(Tab.SearchResults);
			Tabs.MapTabToType<SettingsTab>(Tab.Settings);
			Tabs.MapTabToType<WebTab>(Tab.Web);
			Tabs.MapTabToType<ToolsTab>(Tab.Tools);
			Tabs.MapTabToType<TankoubonsTab>(Tab.Tankoubons);
			Tabs.MapTabToType<TankoubonTab>(Tab.Tankoubon);
			Tabs.MapTabToType<TankoubonEditTab>(Tab.TankoubonEdit);

			MapDialogToType<CategoryArchive>(Dialog.CategoryArchive);
			MapDialogToType<CreateCategory>(Dialog.CreateCategory);
			MapDialogToType<ProgressConflict>(Dialog.ProgressConflict);
			MapDialogToType<ServerProfileDialog>(Dialog.ServerProfile);
			MapDialogToType<ValidateApiDialog>(Dialog.ValidateApi);
			MapDialogToType<ThumbnailPicker>(Dialog.ThumbnailPicker);
			MapDialogToType<CreateTankoubon>(Dialog.CreateTankoubon);
			MapDialogToType<MarkdownDialog>(Dialog.Markdown);

			MapSymbolToSymbol(Symbol.Favorite, new SymbolIconSource { Symbol = Windows.UI.Xaml.Controls.Symbol.Favorite });
			MapSymbolToSymbol(Symbol.Pictures, new SymbolIconSource { Symbol = Windows.UI.Xaml.Controls.Symbol.Pictures });

			Window.Current.Activated += Current_Activated;
		}

		public override Version Version => Package.Current.Id.Version.ToVersion();

		public override bool AnimationsEnabled => _animationsEnabled;

		public override uint HoverTime => UISettings.MouseHoverTime;

		public override bool DualScreen => _dualScreen;

		public override double DualScreenWidth => _dualScreenWidth;

		public override Task<bool> OpenInBrowser(Uri uri) => Launcher.LaunchUriAsync(uri).AsTask();

		public override string GetLocalizedString(string key)
		{
			if (LocalizationCache.TryGetValue(key, out var val))
				return val;
			var split = key.Split(['/'], 2);
			return LocalizationCache[key] = ResourceLoader.GetForCurrentView(split[0]).GetString(split[1]);
		}

		public override async Task<IDialogResult> OpenGenericDialog(string title, string primarybutton, string secondarybutton, string closebutton, object? content)
		{
			await DialogSemaphore.WaitAsync();
			try
			{
				var dialog = new GenericDialog { Title = title, PrimaryButtonText = primarybutton, SecondaryButtonText = secondarybutton, CloseButtonText = closebutton, Content = content };
				return (IDialogResult)(int)await dialog.ShowAsync();
			}
			finally
			{
				DialogSemaphore.Release();
			}
		}

		public override void GoToPage(Pages page, PagesTransition transition, object? parameter = null) => Root.FrameContent.Navigate(GetPage(page), parameter, CreateTransition<NavigationTransitionInfo>(transition));

		public override void CopyToClipboard(string text)
		{
			var dataPackage = new DataPackage();
			dataPackage.RequestedOperation = DataPackageOperation.Copy;
			dataPackage.SetText(text);
			Clipboard.SetContent(dataPackage);
		}

		public override void ChangeTheme(AppTheme theme) => Root.ChangeTheme(Theme = theme);

		public string GetPackageFamilyName() => Package.Current.Id.FamilyName;

		public override async Task<bool> CheckAppInstalled(string package)
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

		public override bool WinRT_IsApiContractPresent(string contractName, ushort majorVersion) => ApiInformation.IsApiContractPresent(contractName, majorVersion);

		public void SetRoot(Root root) => this.Root = root;

		private void Current_Activated(object sender, WindowActivatedEventArgs e) => Active = e.WindowActivationState != CoreWindowActivationState.Deactivated;

	}
}
