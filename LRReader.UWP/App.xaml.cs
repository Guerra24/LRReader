using LRReader.Shared.Services;
using LRReader.UWP.Views;
using LRReader.UWP.Views.Main;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP
{

	sealed partial class App : Application
	{

		public App()
		{
			AppCenter.Start(Secrets.AppCenterId, typeof(Crashes));
			Init.EarlyInit();
			switch (Settings.Theme)
			{
				case AppTheme.Dark:
					this.RequestedTheme = ApplicationTheme.Dark;
					break;
				case AppTheme.Light:
					this.RequestedTheme = ApplicationTheme.Light;
					break;
				case AppTheme.System:
					break;
			}
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		private void UpdateButtonsOnThemeChange(ApplicationTheme theme)
		{
			var AppView = ApplicationView.GetForCurrentView();
			var titleBar = AppView.TitleBar;
			switch (theme)
			{
				case ApplicationTheme.Light:
					titleBar.ButtonForegroundColor = Colors.Black;
					break;
				case ApplicationTheme.Dark:
					titleBar.ButtonForegroundColor = Colors.White;
					break;
			}
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
			Root rootFrame = Window.Current.Content as Root;

			if (rootFrame == null)
			{
				var CoreView = CoreApplication.GetCurrentView();
				var AppView = ApplicationView.GetForCurrentView();
				var coreTitleBar = CoreView.TitleBar;
				coreTitleBar.ExtendViewIntoTitleBar = true;

				var titleBar = AppView.TitleBar;
				titleBar.ButtonBackgroundColor = Colors.Transparent;
				titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

				rootFrame = new Root();
				rootFrame.ActualThemeChanged += (sender, args) => UpdateButtonsOnThemeChange(RequestedTheme);
				UpdateButtonsOnThemeChange(RequestedTheme);

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}
				Window.Current.Content = rootFrame;
			}

			if (!e.PrelaunchActivated)
			{
				CoreApplication.EnablePrelaunch(true);
				if (rootFrame.Frame.Content == null)
					rootFrame.Frame.Navigate(typeof(LoadingPage), e.SplashScreen, new SuppressNavigationTransitionInfo());
				Window.Current.Activate();
			}
			else
			{
				await InitServices();
			}
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
	}
}
