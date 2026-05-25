using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using LRReader.Avalonia.Resources;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using static LRReader.Shared.Services.Service;

namespace LRReader.Avalonia.Views.Main
{
	public partial class HostTabPage : UserControl, IRecipient<ShowNotification>
	{
		private TabsService Data;

		private ResourceLoader lang;

		private WindowState WindowState;

		private TopLevel TopLevel = null!;

		public HostTabPage()
		{
			InitializeComponent();
			DataContext = Data = Service.Services.GetRequiredService<TabsService>();

			lang = ResourceLoader.GetForCurrentView("Pages");
			AddHandler(FAFrame.NavigatedToEvent, OnNavigatedTo);
			AddHandler(FAFrame.NavigatingFromEvent, OnNavigatingFrom);
		}

		protected async void OnNavigatedTo(object? sender, FANavigationEventArgs e)
		{
			WeakReferenceMessenger.Default.Register(this);

			TopLevel = TopLevel.GetTopLevel(this)!;

			TopLevel.BackRequested += HostTabPage_BackRequested;

			var insets = TopLevel.InsetsManager;
			if (insets != null)
			{
				insets.SafeAreaChanged += TitleBar_LayoutMetricsChanged;
				TabViewControl.Padding = insets.SafeAreaPadding;
			}

			if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				desktop.MainWindow!.PropertyChanged += MainWindow_PropertyChanged;

			Platform.ToggleFullScreenModeRequested += Platform_ToggleFullScreenModeRequested;

			Data.OpenTab(Tab.Archives);
			/*if (Settings.OpenBookmarksTab)
				Data.AddTab(new BookmarksTab(), false);
			if (Api.ControlFlags.CategoriesEnabled)
				if (Settings.OpenCategoriesTab)
					Data.AddTab(new CategoriesTab(), false);*/

			var info = await Updates.CheckForUpdates();

			if (info.Result)
				ShowNotification(lang.GetString("HostTab/Update1").AsFormat(info.Target?.ToString() ?? "Nightly"), lang.GetString("HostTab/Update2"), 0, NotificationSeverity.Informational);

			await ShowWhatsNew();
		}

		private void OnNavigatingFrom(object? sender, FANavigatingCancelEventArgs e)
		{
			Platform.ToggleFullScreenModeRequested -= Platform_ToggleFullScreenModeRequested;

			if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				desktop.MainWindow!.PropertyChanged -= MainWindow_PropertyChanged;

			var insets = TopLevel.InsetsManager;
			insets?.SafeAreaChanged -= TitleBar_LayoutMetricsChanged;

			TopLevel.BackRequested -= HostTabPage_BackRequested;

			WeakReferenceMessenger.Default.UnregisterAll(this);
		}

		private void HostTabPage_BackRequested(object? sender, RoutedEventArgs e) => e.Handled = Data.CurrentTab!.BackRequested();

		private void TitleBar_LayoutMetricsChanged(object? sender, SafeAreaChangedArgs e) => TabViewControl.Padding = e.SafeAreaPadding;

		public void Receive(ShowNotification message) => ShowNotification(message.Value.Title, message.Value.Content, message.Value.Duration, message.Value.Severity);

		private void ShowNotification(string title, string? content, int duration, NotificationSeverity severity = NotificationSeverity.Informational)
		{
			//Service.Dispatcher.Run(() => TabViewControl.Notifications.Show(new CommunityToolkit.WinUI.Behaviors.Notification { Title = title, Message = content, Duration = duration <= 0 ? null : TimeSpan.FromMilliseconds(duration), Severity = (InfoBarSeverity)(int)severity }), 0);
			// This is temporary
			Service.Dispatcher.Run(() =>
			{
				Notifications.Title = title;
				Notifications.Message = content;
				Notifications.Severity = (FAInfoBarSeverity)(int)severity;
				Notifications.IsOpen = true;

				if (duration > 0)
				{
					Task.Delay(duration).ContinueWith(_ =>
					{
						Service.Dispatcher.Run(() =>
						{
							Notifications.IsOpen = false;
						});
					});
				}
			});
		}

		private void EnterFullScreen_Click(object? sender, RoutedEventArgs e) => Platform.ToggleFullScreenMode();

		private void RestoreFullScreen_Click(object? sender, RoutedEventArgs e) => Platform.ToggleFullScreenMode();

		private void TabView_TabCloseRequested(FATabView sender, FATabViewTabCloseRequestedEventArgs args) => Data.CloseTab((ModernTab)args.Tab);

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

			await Platform.OpenDialog(Dialog.Markdown, lang.GetString("HostTab/ChangelogTitle"), log.Content);
		}

		private async void RestoreSession_ActionButtonClick(FATeachingTip sender, EventArgs args)
		{
			sender.IsOpen = false;
			await Session.Restore();
		}

		private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property == Window.WindowStateProperty)
			{
				Data.Fullscreen = ((WindowState)e.NewValue!) == WindowState.FullScreen;
			}
		}

		private void Platform_ToggleFullScreenModeRequested()
		{
			if (!Data.Fullscreen)
			{
				if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{
					var window = desktop.MainWindow!;
					WindowState = window.WindowState;
					window.WindowState = WindowState.FullScreen;
				}
				else
				{
					TopLevel.InsetsManager?.IsSystemBarVisible = false;
				}
				Data.Fullscreen = true;
			}
			else
			{
				if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
					desktop.MainWindow!.WindowState = WindowState;
				else
				{
					TopLevel.InsetsManager?.IsSystemBarVisible = true;
				}
				Data.Fullscreen = false;
			}
		}

	}
}
