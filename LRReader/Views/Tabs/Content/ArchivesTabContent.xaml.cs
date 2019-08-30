using LRReader.Internal;
using LRReader.Models.Main;
using LRReader.ViewModels;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Tabs.Content
{
	public sealed partial class ArchivesTabContent : UserControl
	{

		private ArchivesPageViewModel Data;

		private bool loaded;

		public ArchivesTabContent()
		{
			this.InitializeComponent();
			Data = DataContext as ArchivesPageViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			await Data.Refresh();
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			Global.EventManager.AddTab(new ArchiveTab(e.ClickedItem as Archive));
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await Data.Refresh();
		}

		public void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				if (!string.IsNullOrEmpty(sender.Text))
				{
					IEnumerable<Archive> listSearch = Data.ArchiveList;
					if (Data.NewOnly)
					{
						listSearch = listSearch.Where(a => a.isnew.Equals("block"));
					}
					var text = sender.Text.ToUpper();
					foreach (var s in text.Split(" "))
					{
						listSearch = listSearch.Where(a => a.title.ToUpper().Contains(s) || a.tags.ToUpper().Contains(s));
					}
					ArchivesGrid.ItemsSource = listSearch;
				}
				else
				{
					ArchivesGrid.ItemsSource = Data.ArchiveList;
				}
			}
		}

		public void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (args.ChosenSuggestion != null)
			{
				// User selected an item from the suggestion list, take an action on it here.
			}
			else
			{
				// Use args.QueryText to determine what to do.
			}
		}

		private void RandomButton_Click(object sender, RoutedEventArgs e)
		{
			var random = new Random();
			var list = ArchivesGrid.ItemsSource as IEnumerable<Archive>;
			var item = list.ElementAt(random.Next(list.Count()));
			Global.EventManager.AddTab(new ArchiveTab(item));
		}

		private void NewOnlyButton_Click(object sender, RoutedEventArgs e)
		{
			if (Data.NewOnly)
			{
				IEnumerable<Archive> listSearch = Data.ArchiveList;
				listSearch = listSearch.Where(a => a.isnew.Equals("block"));
				ArchivesGrid.ItemsSource = listSearch;
			}
			else
			{
				ArchivesGrid.ItemsSource = Data.ArchiveList;
			}
		}
	}
}
