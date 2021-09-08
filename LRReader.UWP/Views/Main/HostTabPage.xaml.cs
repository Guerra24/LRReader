using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using LRReader.UWP.Views.Controls;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Items;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.Views.Main
{
	public sealed partial class HostTabPage : Page, IRecipient<ShowNotification>
	{

		private readonly TabsService Data;
		private readonly SettingsService Settings;

		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		private ResourceLoader lang;

		public HostTabPage()
		{
			this.InitializeComponent();
			Data = DataContext as TabsService;
			Settings = Service.Settings;

			CoreView = CoreApplication.GetCurrentView();
			AppView = ApplicationView.GetForCurrentView();
			lang = ResourceLoader.GetForCurrentView("Pages");
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			CoreView.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
			CoreView.TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
			AppView.VisibleBoundsChanged += AppView_VisibleBoundsChanged;

			SystemNavigationManager.GetForCurrentView().BackRequested += HostTabPage_BackRequested;
			Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

			TabViewStartHeader.Margin = new Thickness(CoreView.TitleBar.SystemOverlayLeftInset, 0, 0, 0);
			TabViewEndHeader.Margin = new Thickness(0, 0, CoreView.TitleBar.SystemOverlayRightInset, 0);

			Data.Fullscreen = AppView.IsFullScreenMode;

			Window.Current.SetTitleBar(TitleBar);

			WeakReferenceMessenger.Default.Register(this);

			await Service.Dispatcher.RunAsync(() =>
			{
				Data.OpenTab(Tab.Archives);
				if (Settings.OpenBookmarksTab)
					Data.OpenTab(Tab.Bookmarks, false);
				if (Settings.OpenCategoriesTab)
					Data.OpenTab(Tab.Categories, false);
			});

			var info = await Updates.CheckForUpdates();
			if (info.Found)
				ShowNotification(lang.GetString("HostTab/Update1").AsFormat(info.Target), lang.GetString("HostTab/Update2"), 0);

			await ShowWhatsNew();
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);

			CoreView.TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
			CoreView.TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
			AppView.VisibleBoundsChanged -= AppView_VisibleBoundsChanged;

			SystemNavigationManager.GetForCurrentView().BackRequested -= HostTabPage_BackRequested;
			Window.Current.CoreWindow.PointerPressed -= CoreWindow_PointerPressed;

			Window.Current.SetTitleBar(null);

			WeakReferenceMessenger.Default.UnregisterAll(this);
		}

		private void HostTabPage_BackRequested(object sender, BackRequestedEventArgs e)
		{
			e.Handled = Data.CurrentTab.BackRequested();
		}

		private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
		{
			if (e.CurrentPoint.Properties.IsXButton1Pressed)
				e.Handled = Data.CurrentTab.BackRequested();
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			//TitleBar.Height = coreTitleBar.Height;
			TabViewStartHeader.Margin = new Thickness(coreTitleBar.SystemOverlayLeftInset, 0, 0, 0);
			TabViewEndHeader.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
		}

		private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
		{
			if (sender.IsVisible)
				TabViewControl.Margin = new Thickness(0, 0, 0, 0);
			else
				TabViewControl.Margin = new Thickness(0, -40, 0, 0);
		}

		public void Receive(ShowNotification message) => ShowNotification(message.Value.Title, message.Value.Content, message.Value.Duration);

		private void ShowNotification(string title, string content, int duration) => Notifications.Show(new NotificationItem(title, content), duration);

		private void SettingsButton_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Settings);

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) => AppView.TryEnterFullScreenMode();

		private void Bookmarks_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Bookmarks);

		private void Categories_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Categories);

		private void Search_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.SearchResults);

		private void Tools_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Tools);

		private void AppView_VisibleBoundsChanged(ApplicationView sender, object args) => Data.Fullscreen = AppView.IsFullScreenMode;

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			Data.CloseTab(args.Tab as ModernTab);
		}

		private void CloseTab_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			Data.CloseCurrentTab();
		}

		private void FullScreen_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			if (AppView.IsFullScreenMode)
			{
				AppView.ExitFullScreenMode();
			}
			else
			{
				AppView.TryEnterFullScreenMode();
			}
		}

		private async Task ShowWhatsNew()
		{
			if (!SettingsStorage.GetObjectLocal("WasUpdated", false))
				return;
			SettingsStorage.DeleteObjectLocal("WasUpdated");
			SettingsStorage.DeleteObjectLocal("_version");

			if (Platform.Version.Revision != 0)
				return;

			var log = await Updates.GetChangelog(Platform.Version);
			if (log == null)
				return;

			var dialog = new MarkdownDialog(lang.GetString("HostTab/ChangelogTitle"), log?.Content);
			await dialog.ShowAsync();
		}
	}
}
