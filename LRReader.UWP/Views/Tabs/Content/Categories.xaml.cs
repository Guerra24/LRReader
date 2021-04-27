using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Views.Dialogs;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

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
				var dialog = new CreateCategory(false);
				var result = await dialog.ShowAsync();
				if (result == ContentDialogResult.Primary)
				{
					var category = await Data.CreateCategory(dialog.CategoryName.Text, dialog.SearchQuery.Text, dialog.Pinned.IsOn);
					if (category != null && string.IsNullOrEmpty(dialog.SearchQuery.Text))
						Service.Events.AddTab(new CategoryEditTab(category));
				}
			}
			else
			{
				Service.Events.AddTab(new SearchResultsTab(e.ClickedItem as Category));
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
