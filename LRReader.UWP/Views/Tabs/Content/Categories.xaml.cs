using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Dialogs;
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
using LRReader.Shared.Models.Main;
using LRReader.Internal;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class Categories : UserControl
	{
		private CategoriesViewModel Data;

		private bool loaded;

		public Categories()
		{
			this.InitializeComponent();
			Data = DataContext as CategoriesViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			await Data.Refresh();
		}
		private async void CategoriesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem is AddNewCategory)
			{
				CreateCategory dialog = new CreateCategory(false);
				var result = await dialog.ShowAsync();
				if (result == ContentDialogResult.Primary)
				{
					await Data.CreateCategory(dialog.CategoryName.Text, dialog.SearchQuery.Text, dialog.Pinned.IsOn);
				}
			}
			else
			{
				Global.EventManager.AddTab(new SearchResultsTab(e.ClickedItem as Category));
			}
		}

		private async void Button_Click(object sender, RoutedEventArgs e) => await Data.Refresh();

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Data.Refresh();
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => await Data.Refresh();

		public async void Refresh() => await Data.Refresh();

	}

	public class CategoryTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StaticTemplate { get; set; }
		public DataTemplate DynamicTemplate { get; set; }
		public DataTemplate AddNewTemplate { get; set; }
		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is AddNewCategory)
				return AddNewTemplate;
			if (item is Category)
				return StaticTemplate;
			return base.SelectTemplateCore(item);
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}

}
