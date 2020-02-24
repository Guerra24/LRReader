using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.ViewModels;
using LRReader.Views.Items;
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

namespace LRReader.Views.Main
{
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
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			CoreView.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
			AppView.VisibleBoundsChanged += AppView_VisibleBoundsChanged;

			TabViewEndHeader.Margin = new Thickness(0, 0, CoreView.TitleBar.SystemOverlayRightInset, 0);

			Window.Current.SetTitleBar(TitleBar);

			Global.EventManager.ShowErrorEvent += ShowError;
			Global.EventManager.AddTabEvent += AddTab;
			Global.EventManager.CloseAllTabsEvent += CloseAllTabs;
			Global.EventManager.CloseTabWithHeaderEvent += CloseTabWithHeader;
			await DispatcherHelper.RunAsync(() =>
			{
				Global.LRRApi.RefreshSettings(Global.SettingsManager.Profile);
				Global.EventManager.AddTab(new ArchivesTab());
				if (Global.SettingsManager.OpenBookmarksTab)
					Global.EventManager.AddTab(new BookmarksTab(), false);
			});
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			CoreView.TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
			AppView.VisibleBoundsChanged -= AppView_VisibleBoundsChanged;

			Window.Current.SetTitleBar(null);
			Global.EventManager.ShowErrorEvent -= ShowError;
			Global.EventManager.AddTabEvent -= AddTab;
			Global.EventManager.CloseAllTabsEvent -= CloseAllTabs;
			Global.EventManager.CloseTabWithHeaderEvent -= CloseTabWithHeader;
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			//TitleBar.Height = coreTitleBar.Height;
			//TitleBarLeft.Margin = new Thickness(coreTitleBar.SystemOverlayLeftInset, 0, 0, 0);
			TabViewEndHeader.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
		}

		private void ShowError(string title, string content) => Notifications.Show(new NotificationItem(title, content), 5000);

		private void SettingsButton_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new SettingsTab());

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) => AppView.TryEnterFullScreenMode();

		private void Bookmarks_Click(object sender, RoutedEventArgs e) => Global.EventManager.AddTab(new BookmarksTab(), true);

		private void AppView_VisibleBoundsChanged(ApplicationView sender, object args) => Data.FullScreen = AppView.IsFullScreenMode;

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			if (args.Tab is CustomTab tab)
				tab.Unload();
			TabViewControl.TabItems.Remove(args.Tab);
		}

		public async void AddTab(CustomTab tab, bool switchToTab)
		{
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
					await DispatcherHelper.RunAsync(() => Data.CurrentTab = tab);
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

		public void CloseTabWithHeader(string id)
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
	}
}
