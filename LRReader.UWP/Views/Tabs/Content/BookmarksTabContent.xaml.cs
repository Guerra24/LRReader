using LRReader.Internal;
using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Items;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Archive = LRReader.Shared.Models.Main.Archive;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class BookmarksTabContent : UserControl
	{
		private BookmarksTabViewModel Data;

		private bool loaded;

		public BookmarksTabContent()
		{
			this.InitializeComponent();
			Data = DataContext as BookmarksTabViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			await Data.Refresh();
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e) => Global.EventManager.AddTab(new ArchiveTab(e.ClickedItem as Archive), true);

		private async void Button_Click(object sender, RoutedEventArgs e) => await Data.Refresh();

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Data.Refresh(false);
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => await Data.Refresh();

		private void ArchivesGrid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			if (args.ItemContainer.ContentTemplateRoot is BookmarkedArchive item)
				if (item.Parallax.Source == null)
					item.Parallax.Source = ArchivesGrid;
		}

		public async void Refresh() => await Data.Refresh();
	}

	public class BookmarkTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CompactTemplate { get; set; }
		public DataTemplate FullTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (Global.SettingsManager.CompactBookmarks)
				return CompactTemplate;
			else
				return FullTemplate;
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}
}
