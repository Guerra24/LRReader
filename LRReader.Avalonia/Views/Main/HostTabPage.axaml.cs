using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Messages;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Views.Main
{
	public partial class HostTabPage : UserControl, IRecipient<ShowNotification>
	{
		private TabsService Data;

		public HostTabPage()
		{
			InitializeComponent();
			Data = (TabsService)DataContext!;
			AddHandler(FAFrame.NavigatedToEvent, OnNavigatedTo);
			AddHandler(FAFrame.NavigatingFromEvent, OnNavigatingFrom);
		}

		protected async void OnNavigatedTo(object? sender, FANavigationEventArgs e)
		{
			WeakReferenceMessenger.Default.Register(this);

			TopLevel.GetTopLevel(this)!.BackRequested += HostTabPage_BackRequested;

			await Service.Dispatcher.RunAsync(() =>
			{
				Data.OpenTab(Tab.Archives);
				/*if (Settings.OpenBookmarksTab)
					Data.AddTab(new BookmarksTab(), false);
				if (Api.ControlFlags.CategoriesEnabled)
					if (Settings.OpenCategoriesTab)
						Data.AddTab(new CategoriesTab(), false);*/
			});
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
		}

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) { }

		private void TabView_TabCloseRequested(FATabView sender, FATabViewTabCloseRequestedEventArgs args)
		{
			Data.CloseTab((ModernTab)args.Tab);
		}
	}
}
