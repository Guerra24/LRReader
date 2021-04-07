using LRReader.Shared.Services;
using LRReader.UWP.Services;
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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
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
			CoreApplication.EnablePrelaunch(true);

			DispatcherService.Init();
			Frame rootFrame = Window.Current.Content as Frame;

			if (rootFrame == null)
			{
				rootFrame = new Frame();
				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}
				Window.Current.Content = rootFrame;
			}

			if (!e.PrelaunchActivated)
			{
				if (rootFrame.Content == null)
				{
					var CoreView = CoreApplication.GetCurrentView();
					var AppView = ApplicationView.GetForCurrentView();
					var coreTitleBar = CoreView.TitleBar;
					coreTitleBar.ExtendViewIntoTitleBar = true;

					var titleBar = AppView.TitleBar;
					titleBar.ButtonBackgroundColor = Colors.Transparent;
					titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

					rootFrame.ActualThemeChanged += (sender, args) => UpdateButtonsOnThemeChange(RequestedTheme);
					UpdateButtonsOnThemeChange(RequestedTheme);
					rootFrame.Navigate(typeof(LoadingPage), e.Arguments, new SuppressNavigationTransitionInfo());
				}
				Window.Current.Activate();
			}
			else
			{
				await InitServices();
			}
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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
