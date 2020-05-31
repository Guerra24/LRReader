using LRReader.Internal;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class SearchResultsTab : CustomTab
	{
		public SearchResultsTab(string query = "")
		{
			this.InitializeComponent();
			CustomTabId = "Search_" + new Random().Next();
			TabContent.Search(query);
		}

		private void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			TabContent.Refresh();
		}
	}
}
