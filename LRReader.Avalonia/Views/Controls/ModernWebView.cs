using Avalonia.Interactivity;
using Avalonia.Media;
using System.Text.RegularExpressions;

namespace LRReader.Avalonia.Views.Controls
{
	public partial class ModernWebView : UserControl
	{
		private static List<string> Allowed = new List<string>() { "/upload", "/batch", "/config", "/config/plugins", "/logs" };

		public string Title
		{
			get => GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<ModernInput, string>("Title", "");

		public event Action? OnCloseRequested;

		private Uri Page = null!;

		private bool Redirect;

		private IWebView WebView;

		public ModernWebView()
		{
			if (CanUseNativeWebView())
			{
				var webview = new NativeWebViewWebView(this);
				Content = webview;
				WebView = webview;
			}
			else
			{
				var webview = new NativeWebDialogWebView(this);
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

		public bool NavigationStarting(IWebView sender, Uri? uri)
		{
			if (!Page.Host.Equals(uri?.Host))
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

		private static bool CanUseNativeWebView()
		{
			return !OperatingSystem.IsLinux();
		}
	}

	public interface IWebView
	{
		public string DocumentTitle { get; }
		public void Navigate(Uri page);
		public void Refresh();

	}

	public partial class NativeWebViewWebView : UserControl, IWebView
	{
		private ModernWebView Modern;
		private NativeWebView WebView;
		private string Css = """
			body, html {
				background: transparent !important;
			}
			p.ip, #return {
				display: none !important;
			}
			""";

		public NativeWebViewWebView(ModernWebView modern)
		{
			Modern = modern;
			WebView = new NativeWebView
			{
				Background = new SolidColorBrush(Colors.Transparent)
			};
			WebView.NavigationStarted += NavigationStarted;
			WebView.NavigationCompleted += NavigationCompleted;
			WebView.NewWindowRequested += NewWindowRequested;
			Content = WebView;
		}

		public string DocumentTitle => "";

		public void Navigate(Uri page) => WebView.Navigate(page);

		public void Refresh() => WebView.Refresh();

		private async void NavigationStarted(object? sender, WebViewNavigationStartingEventArgs args)
		{
			var res = Modern.NavigationStarting(this, args.Request);
			args.Cancel = res;
		}

		private void NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs args)
		{
			Modern.NavigationCompleted(this, args.IsSuccess);
			if (args.IsSuccess)
			{
				WebView.InvokeScript($$"""
					var style = document.createElement('style');
					style.innerHTML = '{{Regex.Replace(Css, @"\t|\n|\r", " ")}}';
					document.head.appendChild(style);
					""");
			}
		}

		private void NewWindowRequested(object? sender, WebViewNewWindowRequestedEventArgs args)
		{
			args.Handled = true;
		}

	}

	public partial class NativeWebDialogWebView : UserControl, IWebView
	{
		private ModernWebView Modern;
		private NativeWebDialog? WebView;
		private string Css = """
			p.ip {
				display: none !important;
			}
			""";

		private Uri? Destination;

		public NativeWebDialogWebView(ModernWebView modern)
		{
			Modern = modern;

			Content = new Grid();
		}

		protected override void OnLoaded(RoutedEventArgs e)
		{
			base.OnLoaded(e);
			WebView = new NativeWebDialog
			{
				Title = "Web View"
			};
			WebView.NavigationStarted += NavigationStarted;
			WebView.NavigationCompleted += NavigationCompleted;
			WebView.NewWindowRequested += NewWindowRequested;
			if (Destination != null)
				WebView.Navigate(Destination);
			WebView.Show(TopLevel.GetTopLevel(this)!);
		}

		protected override void OnUnloaded(RoutedEventArgs e)
		{
			WebView?.NavigationStarted -= NavigationStarted;
			WebView?.NavigationCompleted -= NavigationCompleted;
			WebView?.NewWindowRequested -= NewWindowRequested;
			WebView?.Close();
			WebView?.Dispose();
			WebView = null;
			base.OnUnloaded(e);
		}

		public string DocumentTitle => "";

		public void Navigate(Uri page)
		{
			if (WebView != null)
				WebView.Navigate(page);
			Destination = page;
		}

		public void Refresh() => WebView?.Refresh();

		private void NavigationStarted(object? sender, WebViewNavigationStartingEventArgs args)
		{
			args.Cancel = Modern.NavigationStarting(this, args.Request);
		}

		private void NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs args)
		{
			Modern.NavigationCompleted(this, args.IsSuccess);
			if (args.IsSuccess)
			{
				WebView?.InvokeScript($$"""
					var style = document.createElement('style');
					style.innerHTML = '{{Regex.Replace(Css, @"\t|\n|\r", " ")}}';
					document.head.appendChild(style);
					""");
			}
		}

		private void NewWindowRequested(object? sender, WebViewNewWindowRequestedEventArgs e)
		{
			e.Handled = true;
		}
	}
}
