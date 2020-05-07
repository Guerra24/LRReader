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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class WebTabContent : UserControl
	{
		public WebTabViewModel ViewModel;

		public WebTabContent()
		{
			
			this.InitializeComponent();
			ViewModel = new WebTabViewModel();
		}

		public void LoadPage(string page)
		{
			WebContent.Navigate(ViewModel.Page = new Uri(page));
		}

		public void RefreshPage()
		{
			WebContent.Refresh();
		}

		private void WebContent_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
		{
			ViewModel.NavigationStarting(sender, args);
		}

		private void WebContent_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
		{
			ViewModel.NavigationCompleted(sender, args);
		}
	}
}
