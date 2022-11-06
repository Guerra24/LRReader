#nullable enable
using LRReader.Shared.Services;
using LRReader.UWP.Services;
using LRReader.UWP.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using static LRReader.Shared.Services.Service;
using ColorHelper = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;

namespace LRReader.UWP
{

	sealed partial class App : Application
	{

		public App()
		{
			AppCenter.Start(Secrets.AppCenterId, typeof(Crashes));
			Init.EarlyInit();
			this.InitializeComponent();
			this.Suspending += OnSuspending;
			this.UnhandledException += App_UnhandledException;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
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
				((UWPlatformService)Platform).SetRoot(root);

				/*if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
					this.Resources["SymbolThemeFontFamily"] = new FontFamily("Segoe Fluent Icons");*/

				((SolidColorBrush)this.Resources["CustomReaderBackground"]).Color = ColorHelper.ToColor(Settings.ReaderBackground);

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}
				Window.Current.Content = root;
			}

			CoreApplication.EnablePrelaunch(true);
			if (root.FrameContent.Content == null)
				Platform.GoToPage(Pages.Loading, PagesTransition.None, e.SplashScreen);
			Window.Current.Activate();
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}

		private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			// Let it go down otherwise we get stuck and that's worse
			if (e.Exception is LayoutCycleException)
				return;
			e.Handled = true;
			Crashes.TrackError(e.Exception);
			// TODO: Do better
			await Service.Platform.OpenGenericDialog("Internal Error", "Continue", content: e.Message);
		}
	}
}
