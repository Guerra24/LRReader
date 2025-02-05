using System;
using System.Linq;
using System.Threading.Tasks;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using LRReader.UWP.Views;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Main;
using LRReader.UWP.Views.Tabs;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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

		private Root? Root;

		public ApplicationExecutionState ExecutionState { get; private set; }

		public UWPlatformService(TabsService tabs, ILoggerFactory loggerFactory, IFilesService files)
		{
			Tabs = tabs;
			_animationsEnabled = UISettings.AnimationsEnabled;

			if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 10))
				UISettings.AnimationsEnabledChanged += (sender, args) => _animationsEnabled = sender.AnimationsEnabled;

			if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
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

			Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "00FFFFFF");

			MapPageToType(Pages.Loading, typeof(LoadingPage));
			MapPageToType(Pages.FirstRun, typeof(FirstRunPage));
			MapPageToType(Pages.HostTab, typeof(HostTabPage));

			MapTransitionToType(PagesTransition.None, typeof(SuppressNavigationTransitionInfo));
			MapTransitionToType(PagesTransition.DrillIn, typeof(DrillInNavigationTransitionInfo));
#if DEBUG || NIGHTLY
			loggerFactory.AddFile(files.LocalCache + string.Format("/Logs/{0:yyyy}-{0:MM}-{0:dd}.log", DateTime.UtcNow));
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
			Tabs.MapTabToType(Tab.Tankoubons, typeof(TankoubonsTab));
			Tabs.MapTabToType(Tab.Tankoubon, typeof(TankoubonTab));
			Tabs.MapTabToType(Tab.TankoubonEdit, typeof(TankoubonEditTab));

			MapDialogToType(Dialog.CategoryArchive, typeof(CategoryArchive));
			MapDialogToType(Dialog.CreateCategory, typeof(CreateCategory));
			MapDialogToType(Dialog.ProgressConflict, typeof(ProgressConflict));
			MapDialogToType(Dialog.ServerProfile, typeof(ServerProfileDialog));
			MapDialogToType(Dialog.ValidateApi, typeof(ValidateApiDialog));
			MapDialogToType(Dialog.ThumbnailPicker, typeof(ThumbnailPicker));
			MapDialogToType(Dialog.CreateTankoubon, typeof(CreateTankoubon));

			MapSymbolToSymbol(Symbol.Favorite, new SymbolIconSource { Symbol = Windows.UI.Xaml.Controls.Symbol.Favorite });
			MapSymbolToSymbol(Symbol.Pictures, new SymbolIconSource { Symbol = Windows.UI.Xaml.Controls.Symbol.Pictures });

			Window.Current.Activated += Current_Activated;
		}

		public override Version Version
		{
			get
			{
				var version = Package.Current.Id.Version;
				return new Version(version.Major, version.Minor, version.Build, version.Revision);
			}
		}

		public override bool AnimationsEnabled => _animationsEnabled;

		public override uint HoverTime => UISettings.MouseHoverTime;

		public override bool DualScreen => _dualScreen;

		public override double DualScreenWidth => _dualScreenWidth;

		public override Task<bool> OpenInBrowser(Uri uri) => Launcher.LaunchUriAsync(uri).AsTask();

		public override string GetLocalizedString(string key)
		{
			var split = key.Split(new[] { '/' }, 2);
			return ResourceLoader.GetForCurrentView(split[0]).GetString(split[1]);
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

		public override void GoToPage(Pages page, PagesTransition transition, object? parameter = null) => Root?.FrameContent.Navigate(GetPage(page), parameter, CreateTransition<NavigationTransitionInfo>(transition));

		public override void CopyToClipboard(string text)
		{
			var dataPackage = new DataPackage();
			dataPackage.RequestedOperation = DataPackageOperation.Copy;
			dataPackage.SetText(text);
			Clipboard.SetContent(dataPackage);
		}

		public override void ChangeTheme(AppTheme theme) => Root?.ChangeTheme(theme);

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

		public void SetRoot(Root root) => this.Root = root;

		private void Current_Activated(object sender, WindowActivatedEventArgs e) => Active = e.WindowActivationState != CoreWindowActivationState.Deactivated;

		public void SetAppExecState(ApplicationExecutionState state) => ExecutionState = state;

	}
}
