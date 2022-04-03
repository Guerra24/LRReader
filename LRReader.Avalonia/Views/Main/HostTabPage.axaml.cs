using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Views.Main
{
	public partial class HostTabPage : UserControl
	{
		private TabsService Data;

		public HostTabPage()
		{
			InitializeComponent();
			Data = DataContext as TabsService;
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

		private void SettingsButton_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Settings);

		private void EnterFullScreen_Click(object sender, RoutedEventArgs e) { }

		private void Bookmarks_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Bookmarks);

		private void Categories_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Categories);

		private void Search_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.SearchResults);

		private void Tools_Click(object sender, RoutedEventArgs e) => Data.OpenTab(Tab.Tools);
	}
}
