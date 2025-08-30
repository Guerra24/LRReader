using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using LRReader.UWP.Extensions;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class CategoryArchive : ContentDialog, IDialog
	{
		private CategoryArchiveViewModel Data;
		private bool _searching;

		public CategoryArchive(string archiveID, string title)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
			Title = title;
			Data = new CategoryArchiveViewModel(archiveID);
			Data.SelectedCategories = CategoriesList.SelectedItems;
		}

		private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
		{
			_searching = true;
			await Data.Load();
			_searching = false;
		}

		private async void CategoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_searching)
				return;
			foreach (var c in e.AddedItems)
				await Data.AddToCategory(((Category)c).id);
			foreach (var c in e.RemovedItems)
				await Data.RemoveFromCategory(((Category)c).id);
			await Data.Reload();
		}

		private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			_searching = true;
			Data.Search(sender.Text);
			_searching = false;
		}

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();
	}
}
