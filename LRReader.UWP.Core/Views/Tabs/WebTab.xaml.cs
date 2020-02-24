using LRReader.Internal;
using Microsoft.UI.Xaml.Controls;
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

namespace LRReader.UWP.Core.Views.Tabs
{
	public sealed partial class WebTab : CustomTab
	{

		private string page;

		private bool loaded;

		public WebTab(string page)
		{
			this.InitializeComponent();
			this.page = page;
			this.CustomTabId = "Web_" + page;
		}

		private void TabViewItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			TabContent.LoadPage(page);
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e) => TabContent.RefreshPage();
	}
}
