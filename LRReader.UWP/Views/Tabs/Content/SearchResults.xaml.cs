﻿using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.UWP.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class SearchResults : UserControl
	{

		private SearchResultsViewModel Data;

		private bool loaded;

		private string query = "";

		public SearchResults()
		{
			this.InitializeComponent();
			Data = new SearchResultsViewModel();
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			AscFlyoutItem.IsChecked = Global.SettingsManager.OrderByDefault == Order.Ascending;
			DesFlyoutItem.IsChecked = Global.SettingsManager.OrderByDefault == Order.Descending;
			await Refresh();
		}

		private void ArchivesGrid_ItemClick(object sender, ItemClickEventArgs e) => Global.EventManager.AddTab(new ArchiveTab(e.ClickedItem as Archive), true);

		private async void Button_Click(object sender, RoutedEventArgs e) => await Refresh();

		public async void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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
						query = sender.Text.Substring(0, Math.Max(0, sQuery.LastIndexOf(" ")));
					}
					foreach (var t in Global.ArchivesManager.TagStats.Where(t => t.GetNamespacedTag().ToUpper().Contains(text)))
					{
						Data.Suggestions.Add(query.TrimEnd() + (string.IsNullOrEmpty(query) ? "" : " ") + t.GetNamespacedTag());
					}
				}
				else
				{
					query = "";
					await HandleSearch();
				}
			}
		}

		public async void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (args.ChosenSuggestion != null)
			{
				query = args.ChosenSuggestion as string;
				await HandleSearch();
			}
			else
			{
				query = args.QueryText;
				await HandleSearch();
			}
		}

		private void RandomButton_Click(object sender, RoutedEventArgs e)
		{
			var random = new Random();
			var list = Global.ArchivesManager.Archives;
			var item = list.ElementAt(random.Next(list.Count() - 1));
			Global.EventManager.AddTab(new ArchiveTab(item));
		}

		private async void FilterToggle_Click(object sender, RoutedEventArgs e) => await Data.ReloadSearch();

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Refresh();
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			await Refresh();
			args.Handled = true;
		}

		private async void PrevButton_Click(object sender, RoutedEventArgs e) => await Data.PrevPage();

		private async void NextButton_Click(object sender, RoutedEventArgs e) => await Data.NextPage();

		private async Task HandleSearch()
		{
			Data.Query = query;
			await Data.ReloadSearch();
		}

		public async Task Refresh()
		{
			Data.ControlsEnabled = false;
			await HandleSearch();
			Data.ControlsEnabled = true;
		}

		private async void ArchivesGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pointerPoint = e.GetCurrentPoint(ArchivesGrid);
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				if (pointerPoint.Properties.IsXButton1Pressed)
				{
					await Data.PrevPage();
				}
				else if (pointerPoint.Properties.IsXButton2Pressed)
				{
					await Data.NextPage();
				}
			}
		}

		public void Search(string query)
		{
			SearchBox.Text = this.query = query;
		}

		public void Search(Category category)
		{
			SearchBox.Text = this.query = category.search;
			Data.Category = category;
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Data.SortByIndex = -1;
		}

		private async void SortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (loaded)
				await HandleSearch();
		}

		private async void OrderBy_Click(object sender, RoutedEventArgs e)
		{
			Data.OrderBy = (Order)Enum.Parse(typeof(Order), (sender as RadioMenuFlyoutItem).Tag as string);
			await HandleSearch();
		}
	}
}
