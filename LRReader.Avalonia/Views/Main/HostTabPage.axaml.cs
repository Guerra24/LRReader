using Avalonia.Controls.ApplicationLifetimes;
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

			TopLevel.GetTopLevel(this)!.BackRequested += HostTabPage_BackRequested;

			Data.OpenTab(Tab.Archives);
			/*if (Settings.OpenBookmarksTab)
				Data.AddTab(new BookmarksTab(), false);
			if (Api.ControlFlags.CategoriesEnabled)
				if (Settings.OpenCategoriesTab)
					Data.AddTab(new CategoriesTab(), false);*/

			var info = await Updates.CheckForUpdates();

			if (info.Result)
				ShowNotification(lang.GetString("HostTab/Update1").AsFormat(info.Target?.ToString() ?? "Nightly"), lang.GetString("HostTab/Update2"), 0, NotificationSeverity.Informational);
		}

		private void OnNavigatingFrom(object? sender, FANavigatingCancelEventArgs e)
		{
			TopLevel.GetTopLevel(this)!.BackRequested -= HostTabPage_BackRequested;

			WeakReferenceMessenger.Default.UnregisterAll(this);
		}
		private void HostTabPage_BackRequested(object? sender, RoutedEventArgs e)
		{
			e.Handled = Data.CurrentTab!.BackRequested();
		}

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

		private void EnterFullScreen_Click(object? sender, RoutedEventArgs e)
		{
			if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				var window = desktop.MainWindow!;
				WindowState = window.WindowState;
				window.WindowState = WindowState.FullScreen;
			}
			Data.Fullscreen = true;
		}

		private void RestoreFullScreen_Click(object? sender, RoutedEventArgs e)
		{
			if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				desktop.MainWindow!.WindowState = WindowState;
			Data.Fullscreen = false;
		}

		private void TabView_TabCloseRequested(FATabView sender, FATabViewTabCloseRequestedEventArgs args)
		{
			Data.CloseTab((ModernTab)args.Tab);
		}
	}
}
