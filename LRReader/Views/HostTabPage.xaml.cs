using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.ViewModels;
using LRReader.Views.Tabs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
	public sealed partial class HostTabPage : Page
	{

		private HostTabPageViewModel Data;

		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		public HostTabPage()
		{
			this.InitializeComponent();

			Data = DataContext as HostTabPageViewModel;

			CoreView = CoreApplication.GetCurrentView();
			AppView = ApplicationView.GetForCurrentView();

			CoreApplicationViewTitleBar coreTitleBar = CoreView.TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;
			//TitleBar.Height = coreTitleBar.Height;
			coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

			ApplicationViewTitleBar titleBar = AppView.TitleBar;
			titleBar.ButtonBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];
			AppView.VisibleBoundsChanged += AppView_VisibleBoundsChanged;

			Window.Current.SetTitleBar(TitleBar);

			Global.EventManager.ShowErrorEvent += ShowError;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await DispatcherHelper.RunAsync(() =>
			{
				bool firstRun = Global.SettingsManager.Profile == null;
				if (firstRun)
				{
					Global.EventManager.AddTab(new FirstRunTab());
				}
				else
				{
					Global.LRRApi.RefreshSettings();
					Global.EventManager.AddTab(new ArchivesTab());
				}
			});
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			//TitleBar.Height = coreTitleBar.Height;
			//TitleBarLeft.Margin = new Thickness(coreTitleBar.SystemOverlayLeftInset, 0, 0, 0);
			TabViewEndHeader.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
		}

		private async void ShowError(string title, string content)
		{
			await DispatcherHelper.RunAsync(async () =>
			{
				ContentDialog noServer = new ContentDialog()
				{
					Title = title,
					Content = content,
					CloseButtonText = "Ok"
				};
				await noServer.ShowAsync();
			});
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.AddTab(new SettingsTab());
		}

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e)
		{
			AppView.TryEnterFullScreenMode();
		}

		private void AppView_VisibleBoundsChanged(ApplicationView sender, object args)
		{
			Data.FullScreen = AppView.IsFullScreenMode;
		}

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			Data.Tabs.Remove(args.Tab);
		}
	}
}
