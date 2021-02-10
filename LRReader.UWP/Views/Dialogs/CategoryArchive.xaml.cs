using LRReader.Shared.Models.Main;
using LRReader.UWP.ViewModels;
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

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class CategoryArchive : ContentDialog
	{

		private CategoryArchiveViewModel Data;
		private bool _searching;

		public CategoryArchive(string archiveID)
		{
			this.InitializeComponent();
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
				await Data.AddToCategory((c as Category).id);
			foreach (var c in e.RemovedItems)
				await Data.RemoveFromCategory((c as Category).id);
			await Data.Reload();
		}

		private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			_searching = true;
			Data.Search(sender.Text);
			_searching = false;
		}
	}
}
