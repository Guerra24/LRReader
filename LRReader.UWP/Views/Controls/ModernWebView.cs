#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LRReader.UWP.Extensions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LRReader.UWP.Views.Controls
{
	public class ModernWebView : UserControl, IDisposable
	{
		private static List<string> Allowed = new List<string>() { "/upload", "/batch", "/config", "/config/plugins", "/logs" };

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			private set => SetValue(TitleProperty, value);
		}

		public event Action? OnCloseRequested;

		private Uri Page = null!;

		private bool Redirect;

		private IWebView WebView;

		public ModernWebView()
		{
			if (CanUseWebView2())
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

		public void Navigate(string url)
		{
			if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
				return;
			Page = new Uri(url);
			WebView.Navigate(Page);
		}

		public void Refresh() => WebView.Refresh();

		public void Dispose() => WebView.Dispose();

		public bool NavigationStarting(IWebView sender, Uri uri)
		{
			if (!Page.Host.Equals(uri.Host))
				return true;
			var path = uri.AbsolutePath;
			if (path.Equals("/login") || Allowed.Contains(path))
			{
				Redirect = true;
			}
			else if (path.Equals("/"))
			{
				OnCloseRequested?.Invoke();
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

		private static bool CanUseWebView2()
		{
			try
			{
				CoreWebView2Environment.GetAvailableBrowserVersionString();
				return true;
			}
			catch { }
			return false;
		}

		public static DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernWebView), new PropertyMetadata(""));
	}

	public interface IWebView : IDisposable
	{
		public string DocumentTitle { get; }
		public Task Navigate(Uri page);
		public void Refresh();

	}

	public class EdgeChromeWebView : UserControl, IWebView
	{
		private ModernWebView Modern;
		private WebView2 WebView;
		private bool Initialized;
		private string Css = """
			body, html {
				background: transparent !important;
			}
			p.ip, #return {
				display: none !important;
			}
			""";

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

		public async Task Navigate(Uri page)
		{
			await WebView.EnsureCoreWebView2Async();
			if (!Initialized)
			{
				Initialized = true;
				WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
				WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
				WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
				WebView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
			}
			WebView.CoreWebView2.Navigate(page.ToString());
		}

		public void Refresh() => WebView.Reload();

		public void Dispose() => WebView.Close();

		private async void NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
		{
			var res = Modern.NavigationStarting(this, new Uri(args.Uri));
			if (!res)
				await WebView.FadeOutAsync();
			args.Cancel = res;
		}

		private void NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
		{
			Modern.NavigationCompleted(this, args.IsSuccess);
		}

		private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
		{
			args.Handled = true;
		}

		private async void CoreWebView2_DOMContentLoaded(CoreWebView2 sender, CoreWebView2DOMContentLoadedEventArgs args)
		{
			await sender.ExecuteScriptAsync($$"""
				var style = document.createElement('style');
				style.innerHTML = '{{Regex.Replace(Css, @"\t|\n|\r", " ")}}';
				document.head.appendChild(style);
				""");
			WebView.FadeIn();
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

		public Task Navigate(Uri page)
		{
			WebView.Navigate(page);
			return Task.CompletedTask;
		}

		public void Refresh() => WebView.Refresh();

		public void Dispose() { }

		private void NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
		{
			args.Cancel = Modern.NavigationStarting(this, args.Uri);
		}

		private void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
		{
			Modern.NavigationCompleted(this, args.IsSuccess);
		}

		private void DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
		{
			//var func = "var style = document.createElement('style'); style.innerHTML = 'body, html { background: transparent !important; } p.ip { display: none !important; } div.ido, .option-flyout { border: 1px solid #00000019; border-radius: 4px; background-color: #FFFFFF0D !important; background-clip: padding-box !important; }'; document.head.appendChild(style);";
			//await sender.InvokeScriptAsync("eval", new string[] { func });
		}
	}
}
