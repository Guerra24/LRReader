using LRReader.Internal;
using LRReader.Models.Main;
using LRReader.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private string query = "";

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
			await Data.LoadTagStats();
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e)
		{
			Global.EventManager.AddTab(new ArchiveTab(e.ClickedItem as Archive));
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await Data.Refresh();
			await Data.LoadTagStats();
			HandleSearch();
		}

		public void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				Data.Suggestions.Clear();
				if (!string.IsNullOrEmpty(sender.Text))
				{
					string text;
					var sQuery = sender.Text.ToUpper();
					if (sender.Text.Length > query.Length)
					{
						text = sQuery.Substring(query.Length).TrimStart();
					}
					else
					{
						text = sQuery.Split(" ").Last();
						query = sender.Text.Substring(0, sQuery.LastIndexOf(" "));
					};
					foreach (var t in Data.TagStats.Where(t => t.text.ToUpper().Contains(text)))
					{
						Data.Suggestions.Add(query.TrimEnd() + (string.IsNullOrEmpty(query) ? "" : " ") + t.text);
					}
				}
				else
				{
					query = "";
					HandleSearch();
				}
			}
		}

		public void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (args.ChosenSuggestion != null)
			{
				query = args.ChosenSuggestion as string;
				HandleSearch();
			}
			else
			{
				query = args.QueryText;
				HandleSearch();
			}
		}

		private void HandleSearch()
		{
			if (!string.IsNullOrEmpty(query))
			{
				IEnumerable<Archive> listSearch = Data.ArchiveList;
				if (Data.NewOnly)
				{
					listSearch = listSearch.Where(a => a.IsNewArchive());
				}
				var text = query.ToUpper();
				foreach (var s in text.Split(" "))
				{
					listSearch = listSearch.Where(a => a.title.ToUpper().Contains(s) || a.tags.ToUpper().Contains(s));
				}
				ArchivesGrid.ItemsSource = listSearch;
			}
			else
			{
				if (Data.NewOnly)
				{
					ArchivesGrid.ItemsSource = Data.ArchiveList.Where(a => a.IsNewArchive());
				}
				else
				{
					ArchivesGrid.ItemsSource = Data.ArchiveList;
				}
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
			HandleSearch();
		}

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Data.Refresh(false);
				await Data.LoadTagStats();
			}
			HandleSearch();
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			await Data.Refresh();
			await Data.LoadTagStats();
			HandleSearch();
		}
	}
}
