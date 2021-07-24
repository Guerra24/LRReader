using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using System;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class SearchResultsTab : ModernTab
	{

		public SearchResultsTab()
		{
			this.InitializeComponent();
			CustomTabId = "Search_" + new Random().Next();
		}

		public SearchResultsTab(string query = "")
		{
			this.InitializeComponent();
			CustomTabId = "Search_" + new Random().Next();
			TabContent.Search(query);
		}

		public SearchResultsTab(Category category)
		{
			this.InitializeComponent();
			CustomTabId = "Search_" + category.id;
			Header = category.name;
			TabContent.Search(category);
		}

		private async void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			await TabContent.Refresh();
		}
	}
}
