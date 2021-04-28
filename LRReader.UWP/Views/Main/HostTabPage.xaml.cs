using LRReader.Internal;
using LRReader.Shared.Services;
using LRReader.UWP.Internal;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Items;
using LRReader.UWP.Views.Tabs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.Views.Main
{
	public sealed partial class HostTabPage : Page
	{

		private TabsService Data;

		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		private ResourceLoader lang;

		public HostTabPage()
		{
			this.InitializeComponent();
			Data = DataContext as TabsService;

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

			TabViewStartHeader.Margin = new Thickness(CoreView.TitleBar.SystemOverlayLeftInset, 0, 0, 0);
			TabViewEndHeader.Margin = new Thickness(0, 0, CoreView.TitleBar.SystemOverlayRightInset, 0);

			Data.FullScreen = AppView.IsFullScreenMode;

			Window.Current.SetTitleBar(TitleBar);

			Events.ShowNotificationEvent += ShowNotification;

			Data.Hook();

			await Service.Dispatcher.RunAsync(() =>
			{
				Data.AddTab(new ArchivesTab());
				if (Settings.OpenBookmarksTab)
					Data.AddTab(new BookmarksTab(), false);
				if (Api.ControlFlags.CategoriesEnabled)
					if (Settings.OpenCategoriesTab)
						Data.AddTab(new CategoriesTab(), false);
			});
			var info = await Global.UpdatesManager.CheckUpdates(Platform.GetVersion());
			if (info != null)
				ShowNotification(lang.GetString("HostTab/Update1") + " " + info.name, lang.GetString("HostTab/Update2"), 0);
#if !SIDELOAD
			await ShowWhatsNew();
#endif
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);

			CoreView.TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
			CoreView.TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
			AppView.VisibleBoundsChanged -= AppView_VisibleBoundsChanged;

			Window.Current.SetTitleBar(null);
			Events.ShowNotificationEvent -= ShowNotification;

			Data.UnHook();
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

		private void ShowNotification(string title, string content, int duration = 5000) => Notifications.Show(new NotificationItem(title, content), duration);

		private void SettingsButton_Click(object sender, RoutedEventArgs e) => Data.AddTab(new SettingsTab());

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) => AppView.TryEnterFullScreenMode();

		private void Bookmarks_Click(object sender, RoutedEventArgs e) => Data.AddTab(new BookmarksTab(), true);

		private void Categories_Click(object sender, RoutedEventArgs e) => Data.AddTab(new CategoriesTab(), true);

		private void Search_Click(object sender, RoutedEventArgs e) => Data.AddTab(new SearchResultsTab(), true);

		private void AppView_VisibleBoundsChanged(ApplicationView sender, object args) => Data.FullScreen = AppView.IsFullScreenMode;

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			Data.CloseTab(args.Tab as CustomTab);
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
			var version = Version.Parse(SettingsStorage.GetObjectLocal("_version", new Version(0, 0, 0, 0).ToString()));
			if (version >= Platform.GetVersion())
				return;
			var result = await Global.UpdatesManager.GetChangelog(Platform.GetVersion());
			if (result == null)
				return;
			SettingsStorage.StoreObjectLocal("_version", Platform.GetVersion().ToString());
			var dialog = new MarkdownDialog(lang.GetString("HostTab/ChangelogTitle"), result);
			await dialog.ShowAsync();
		}

	}
}
