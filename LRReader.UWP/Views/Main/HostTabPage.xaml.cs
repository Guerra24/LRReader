﻿using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using LRReader.UWP.Services;
using LRReader.UWP.Views.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
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
			Data = Service.Services.GetRequiredService<TabsService>();
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

			if (((UWPlatformService)Service.Platform).ExecutionState == ApplicationExecutionState.Terminated)
				await Service.Persistance.Restore();

			var info = await Updates.CheckForUpdates();

			if (info.Result)
				ShowNotification(lang.GetString("HostTab/Update1").AsFormat(info.Target?.ToString() ?? "Nightly"), lang.GetString("HostTab/Update2"), 0, NotificationSeverity.Informational);

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

		private void HostTabPage_BackRequested(object? sender, BackRequestedEventArgs e)
		{
			e.Handled = Data.CurrentTab!.BackRequested();
		}

		private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
		{
			if (e.CurrentPoint.Properties.IsXButton1Pressed)
				e.Handled = Data.CurrentTab!.BackRequested();
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
				TabViewControl.Margin = new Thickness(0, -48, 0, 0);
		}

		public void Receive(ShowNotification message) => ShowNotification(message.Value.Title, message.Value.Content, message.Value.Duration, message.Value.Severity);

		private void ShowNotification(string title, string? content, int duration, NotificationSeverity severity = NotificationSeverity.Informational)
		{
			Service.Dispatcher.Run(() => TabViewControl.Notifications.Show(new CommunityToolkit.WinUI.Behaviors.Notification { Title = title, Message = content, Duration = duration <= 0 ? null : TimeSpan.FromMilliseconds(duration), Severity = (InfoBarSeverity)(int)severity }), 0);
		}

		// Move all of this to the ViewModel
		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) => AppView.TryEnterFullScreenMode();

		private void AppView_VisibleBoundsChanged(ApplicationView sender, object args) => Data.Fullscreen = AppView.IsFullScreenMode;

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			Data.CloseTab((ModernTab)args.Tab);
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

		// Move this to the VM layer
		private async Task ShowWhatsNew()
		{
			var ver = Version.Parse(SettingsStorage.GetObjectLocal(new Version(0, 0, 0, 0).ToString(), "_version"));
			if (!SettingsStorage.GetObjectLocal(false, "WasUpdated") && ver == new Version(0, 0, 0, 0))
				return;
			SettingsStorage.DeleteObjectLocal("WasUpdated");
			SettingsStorage.DeleteObjectLocal("_version");

			if (Platform.Version.Revision != 0)
				return;

			var log = await Updates.GetChangelog(Platform.Version);
			if (string.IsNullOrEmpty(log.Name) || string.IsNullOrEmpty(log.Content))
				return;

			var dialog = Platform.CreateDialog(Dialog.Markdown, lang.GetString("HostTab/ChangelogTitle"), log.Content);
			await dialog.ShowAsync();
		}
	}
}
