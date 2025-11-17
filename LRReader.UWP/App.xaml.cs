using LRReader.Shared.Services;
using LRReader.UWP.Services;
using LRReader.UWP.Views;
using Sentry;
using Sentry.Protocol;
using System;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using WinRT;
using static LRReader.Shared.Services.Service;
using ColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;
using UnhandledExceptionEventArgs = Windows.UI.Xaml.UnhandledExceptionEventArgs;

namespace LRReader.UWP
{

	sealed partial class App : Application
	{

		public App()
		{
			Thread.CurrentThread.Name = "Application";
			Init.EarlyInit();
			// Manually read setting to prevent services from initializing too early
			var crashReporting = ApplicationData.Current.LocalSettings.Values["CrashReporting"];
			if ((bool)(crashReporting ?? true) && Uri.IsWellFormedUriString(Secrets.SentryDsn, UriKind.Absolute))
			{
#if !DEBUG
				SentrySdk.Init(options =>
				{
					options.Dsn = Secrets.SentryDsn;
					options.IsGlobalModeEnabled = true;
#if SIDELOAD
					options.Distribution = "sideload";
					options.Environment = "stable";
#elif NIGHTLY
					options.Distribution = "sideload";
					options.Environment = "nightly";
#else
					options.Distribution = "store";
					options.Environment = "stable";
#endif
					options.AutoSessionTracking = true;
					var ver = Package.Current.Id.Version;
					options.Release = $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
					options.CacheDirectoryPath = Files.Local;
				});
				SentrySdk.ConfigureScope(scope =>
				{
					Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation deviceInfo = new();
					var versionInfo = Windows.System.Profile.AnalyticsInfo.VersionInfo;
					ulong v = ulong.Parse(versionInfo.DeviceFamilyVersion);
					ulong v1 = (v & 0xFFFF000000000000L) >> 48;
					ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
					ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
					ulong v4 = (v & 0x000000000000FFFFL);

					if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 12))
						scope.Contexts.OperatingSystem.Name = versionInfo.ProductName;
					else
						scope.Contexts.OperatingSystem.Name = deviceInfo.OperatingSystem;
					scope.Contexts.OperatingSystem.Version = $"{v1}.{v2}.{v3}";
					scope.Contexts.OperatingSystem.Build = $"{v4}";
					scope.Contexts.Device.Architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString();
					scope.Contexts.Device.ProcessorCount = Environment.ProcessorCount;
					scope.Contexts.Device.Family = versionInfo.DeviceFamily;
					scope.Contexts.Device.Model = string.IsNullOrEmpty(deviceInfo.SystemSku) ? deviceInfo.SystemProductName : deviceInfo.SystemSku;
				});
#endif
			}
			this.UnhandledException += App_UnhandledException;
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		[DynamicWindowsRuntimeCast(typeof(SolidColorBrush))]
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			Root? root = Window.Current.Content as Root;

			if (root == null)
			{
				var CoreView = CoreApplication.GetCurrentView();
				var AppView = ApplicationView.GetForCurrentView();
				var coreTitleBar = CoreView.TitleBar;
				coreTitleBar.ExtendViewIntoTitleBar = true;

				var titleBar = AppView.TitleBar;
				titleBar.ButtonBackgroundColor = Colors.Transparent;
				titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

				root = new Root();
				root.ChangeTheme(Settings.Theme);
				root.UpdateThemeColors();
				var platform = (UWPlatformService)Platform;
				platform.SetRoot(root);

				((SolidColorBrush)this.Resources["CustomReaderBackground"]).Color = ColorHelper.ToColor(Settings.ReaderBackground);

				Window.Current.Content = root;
			}

			CoreApplication.EnablePrelaunch(true);
			if (root.FrameContent.Content == null)
				Platform.GoToPage(Pages.Loading, PagesTransition.None, e.SplashScreen);
			Window.Current.Activate();
		}

		[DynamicWindowsRuntimeCast(typeof(AppServiceTriggerDetails))]
		protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
		{
			if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails)
				Karen.Connect(args.TaskInstance);
		}

		private async void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			await Session.Suspend();
			await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
			deferral.Complete();
		}

		private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			// Let it go down otherwise we get stuck and that's worse
			var exception = e.Exception;
			if (exception is LayoutCycleException)
				return;
			e.Handled = true;
			exception.Data[Mechanism.HandledKey] = false;
			exception.Data[Mechanism.MechanismKey] = "Application.UnhandledException";

			SentrySdk.CaptureException(exception);

			SentrySdk.Flush(TimeSpan.FromSeconds(2));

			Dispatcher.Run(async () => await Platform.OpenGenericDialog("Internal Error", "Continue", content: exception.Message));
		}
	}
}
