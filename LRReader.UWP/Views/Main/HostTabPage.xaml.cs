using LRReader.Internal;
using LRReader.Shared.Services;
using LRReader.UWP.Internal;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Dialogs;
using LRReader.UWP.Views.Items;
using LRReader.UWP.Views.Tabs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
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

		private HostTabPageViewModel Data;

		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		private ResourceLoader lang;

		public HostTabPage()
		{
			this.InitializeComponent();
			Data = DataContext as HostTabPageViewModel;

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
			Events.AddTabEvent += AddTab;
			Events.CloseAllTabsEvent += CloseAllTabs;
			Events.CloseTabWithIdEvent += CloseTabWithId;
			Events.DeleteArchiveEvent += DeleteArchive;
			await Service.Dispatcher.RunAsync(() =>
			{
				Events.AddTab(new ArchivesTab());
				if (Settings.OpenBookmarksTab)
					Events.AddTab(new BookmarksTab(), false);
				if (Api.ControlFlags.CategoriesEnabled)
					if (Settings.OpenCategoriesTab)
						Events.AddTab(new CategoriesTab(), false);
			});
			var info = await Global.UpdatesManager.CheckUpdates(Util.GetAppVersion());
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
			Events.AddTabEvent -= AddTab;
			Events.CloseAllTabsEvent -= CloseAllTabs;
			Events.CloseTabWithIdEvent -= CloseTabWithId;
			Events.DeleteArchiveEvent -= DeleteArchive;
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

		private void SettingsButton_Click(object sender, RoutedEventArgs e) => Events.AddTab(new SettingsTab());

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) => AppView.TryEnterFullScreenMode();

		private void Bookmarks_Click(object sender, RoutedEventArgs e) => Events.AddTab(new BookmarksTab(), true);

		private void Categories_Click(object sender, RoutedEventArgs e) => Events.AddTab(new CategoriesTab(), true);

		private void Search_Click(object sender, RoutedEventArgs e) => Events.AddTab(new SearchResultsTab(), true);

		private void AppView_VisibleBoundsChanged(ApplicationView sender, object args) => Data.FullScreen = AppView.IsFullScreenMode;

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			if (args.Tab is CustomTab tab)
				tab.Unload();
			TabViewControl.TabItems.Remove(args.Tab);
		}

		public async void AddTab(object tabo, bool switchToTab)
		{
			var tab = tabo as CustomTab;
			var current = GetTabFromId(tab.CustomTabId);
			if (current != null)
			{
				if (switchToTab)
					Data.CurrentTab = current;
			}
			else
			{
				TabViewControl.TabItems.Add(tab);
				if (switchToTab)
					await Service.Dispatcher.RunAsync(() => Data.CurrentTab = tab);
			}
		}

		public void CloseAllTabs()
		{
			foreach (var t in TabViewControl.TabItems)
			{
				if (t is CustomTab tab)
					tab.Unload();
			}
			TabViewControl.TabItems.Clear();
		}

		public void CloseTabWithId(string id)
		{
			var tab = GetTabFromId(id);
			if (tab != null)
			{
				TabViewControl.TabItems.Remove(tab);
			}
		}

		private CustomTab GetTabFromId(string id) => TabViewControl.TabItems.FirstOrDefault(t => (t as CustomTab).CustomTabId.Equals(id)) as CustomTab;

		private void CloseTab_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			args.Handled = true;
			var t = Data.CurrentTab;
			if (!t.IsClosable)
				return;
			if (t is CustomTab tab)
				tab.Unload();
			TabViewControl.TabItems.Remove(t);
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

		public void DeleteArchive(string id)
		{
			CloseTabWithId("Edit_" + id);
			CloseTabWithId("Archive_" + id);
		}

		private async Task ShowWhatsNew()
		{
			var version = Version.Parse(SettingsStorage.GetObjectLocal("_version", new Version(0, 0, 0, 0).ToString()));
			if (version >= Util.GetAppVersion())
				return;
			var result = await Global.UpdatesManager.GetChangelog(Util.GetAppVersion());
			if (result == null)
				return;
			SettingsStorage.StoreObjectLocal("_version", Util.GetAppVersion().ToString());
			var dialog = new MarkdownDialog(lang.GetString("HostTab/ChangelogTitle"), result);
			await dialog.ShowAsync();
		}

	}
}
