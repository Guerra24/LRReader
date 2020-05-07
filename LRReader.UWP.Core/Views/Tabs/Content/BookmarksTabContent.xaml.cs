using LRReader.Internal;
using Archive = LRReader.Shared.Models.Main.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LRReader.UWP.Views.Items;
using LRReader.UWP.ViewModels;

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

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e) => Global.EventManager.AddTab(new ArchiveTab(e.ClickedItem as Archive), Global.SettingsManager.SwitchTabArchive);

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
			var item = args.ItemContainer.ContentTemplateRoot as BookmarkedArchive;
			if (item.Parallax.Source == null)
				item.Parallax.Source = ArchivesGrid;
		}

		public async void Refresh() => await Data.Refresh();
	}
}
