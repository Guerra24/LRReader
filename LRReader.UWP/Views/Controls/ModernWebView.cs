#nullable enable
using System;
using System.Collections.Generic;
using LRReader.Shared.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LRReader.UWP.Views.Controls
{
	public class ModernWebView : UserControl
	{
		private static List<string> Allowed = new List<string>() { "/upload", "/batch", "/config", "/config/plugins", "/logs" };

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			private set => SetValue(TitleProperty, value);
		}

		private Uri Page = null!;

		private bool Redirect;

		private string? TabId;

		private IWebView WebView;

		public ModernWebView()
		{
			if (Init.CanUseWebView2())
			{
				var webview = new EdgeChromeWebView(this);
				Content = webview;
				WebView = webview;
			}
			else
			{
				var webview = new EdgeHTMLWebView(this);
				Content = webview;
				WebView = webview;
			}
		}

		public void Navigate(string url, string tabId)
		{
			if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
				return;
			TabId = tabId;
			Page = new Uri(url);
			WebView.Navigate(Page);
		}

		public void Refresh() => WebView.Refresh();

		public void Close() => WebView.Close();

		public bool NavigationStarting(IWebView sender, Uri uri)
		{
			var path = uri.AbsolutePath;
			if (path.Equals("/login") || Allowed.Contains(path))
			{
				Redirect = true;
			}
			else if (path.Equals("/"))
			{
				Service.Tabs.CloseTabWithId(TabId);
				return true;
			}
			else if (path.Equals(Page.AbsolutePath))
			{
				Title = "Loading...";
			}
			else
			{
				if (Redirect)
				{
					Redirect = false;
					sender.Navigate(Page);
				}
				else
				{
					return true;
				}
			}
			return false;
		}

		public void NavigationCompleted(IWebView sender, bool success)
		{
			if (success)
			{
				Title = sender.DocumentTitle;
			}
			else
			{
				// Show Error
			}
		}

		public static DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernWebView), new PropertyMetadata(""));
	}

	public interface IWebView
	{
		public string DocumentTitle { get; }
		public void Navigate(Uri page);
		public void Refresh();
		public void Close();
	}

	public class EdgeChromeWebView : UserControl, IWebView
	{
		private ModernWebView Modern;
		private WebView2 WebView;
		private bool Initialized;

		public EdgeChromeWebView(ModernWebView modern)
		{
			Modern = modern;
			WebView = new WebView2();
			WebView.Background = new SolidColorBrush(Colors.Transparent);
			WebView.NavigationStarting += NavigationStarting;
			WebView.NavigationCompleted += NavigationCompleted;
			Content = WebView;
		}

		public string DocumentTitle => WebView.CoreWebView2.DocumentTitle;

		public async void Navigate(Uri page)
		{
			await WebView.EnsureCoreWebView2Async();
			if (!Initialized)
			{
				Initialized = true;
				WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
				WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
				WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
			}
			WebView.Source = page;
		}

		public void Refresh() => WebView.Reload();

		public void Close() => WebView.Close();

		private void NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args) => args.Cancel = Modern.NavigationStarting(this, new Uri(args.Uri));

		private void NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args) => Modern.NavigationCompleted(this, args.IsSuccess);

		private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
		{
			args.Handled = true;
		}

	}

	public class EdgeHTMLWebView : UserControl, IWebView
	{
		private ModernWebView Modern;
		private WebView WebView;

		public EdgeHTMLWebView(ModernWebView modern)
		{
			Modern = modern;
			WebView = new WebView { DefaultBackgroundColor = Colors.Transparent };
			WebView.NavigationStarting += NavigationStarting;
			WebView.NavigationCompleted += NavigationCompleted;
			WebView.DOMContentLoaded += DOMContentLoaded;
			Content = WebView;
		}

		public string DocumentTitle => WebView.DocumentTitle;

		public void Navigate(Uri page) => WebView.Navigate(page);

		public void Refresh() => WebView.Refresh();

		public void Close() { }

		private void NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) => args.Cancel = Modern.NavigationStarting(this, args.Uri);

		private void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) => Modern.NavigationCompleted(this, args.IsSuccess);

		private async void DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
		{
			string func = "var style = document.createElement('style'); style.innerHTML = 'body, html { background: transparent !important; } p.ip { display: none !important; }'; document.head.appendChild(style);";
			await sender.InvokeScriptAsync("eval", new string[] { func });
		}
	}
}
