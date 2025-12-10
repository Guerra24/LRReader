using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Views.Main
{
	public partial class HostTabPage : UserControl
	{
		private TabsService Data;

		public HostTabPage()
		{
			InitializeComponent();
			Data = (TabsService)DataContext!;
		}

		private async void HostTabPage_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			//WeakReferenceMessenger.Default.Register(this);

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

		private void HostTabPage_DetachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			//WeakReferenceMessenger.Default.UnregisterAll(this);
		}

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) { }

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			Data.CloseTab((CustomTab)args.Tab);
		}
	}
}
