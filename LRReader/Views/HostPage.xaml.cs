using LRReader.Views.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class HostPage : Page
	{
		public HostPage()
		{
			this.InitializeComponent();
			CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;
			TitleBar.Height = coreTitleBar.Height;
			Window.Current.SetTitleBar(MainTitleBar);
			coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
			coreTitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
			Window.Current.Activated += TitleBar_Activated;

			ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
			titleBar.ButtonBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];

			// Load index page
			ContentFrame.Navigate(typeof(FirstRun));
			ContentFrame.Navigated += OnContentNavigatedTo;
			SystemNavigationManager.GetForCurrentView().BackRequested += OnContentBackRequested;
		}

		private void OnContentNavigatedTo(object sender, NavigationEventArgs e)
		{
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
		}

		private void OnContentBackRequested(object sender, BackRequestedEventArgs e)
		{
			ContentFrame.GoBack();
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			TitleBar.Height = coreTitleBar.Height;
			TitleBarLeft.Margin = new Thickness(coreTitleBar.SystemOverlayLeftInset, 0, 0, 0);
			TitleBarRight.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
		}

		private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
		{
			if (sender.IsVisible)
			{
				TitleBar.Visibility = Visibility.Visible;
			}
			else
			{
				TitleBar.Visibility = Visibility.Collapsed;
			}
		}
		private void TitleBar_Activated(object sender, WindowActivatedEventArgs e)
		{
			SolidColorBrush foreground = null;
			if (e.WindowActivationState != CoreWindowActivationState.Deactivated)
				foreground = new SolidColorBrush((Color)this.Resources["SystemBaseHighColor"]);
			else
				foreground = new SolidColorBrush(Colors.Gray);
			AppTitle.Foreground = foreground;
		}
	}
}
