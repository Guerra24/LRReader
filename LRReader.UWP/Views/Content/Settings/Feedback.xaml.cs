using System;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{

	public sealed partial class Feedback : Page
	{
		public Feedback()
		{
			this.InitializeComponent();
		}

		private async void WebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
		{
			string func = "var style = document.createElement('style'); style.innerHTML = '#form-container { width: unset !important; } body, html { background: transparent !important; } .office-form-page-padding { padding-left: unset !important; } .office-form-body { opacity: 1 !important; } .office-form-theme-footer, .office-form-notice-report, .office-form.introduction-for-responser-container { display: none !important; } .office-form-content { padding-top: 0px !important; overflow: unset !important; } #content-root { height: unset !important; margin: auto !important; } body { overflow: auto !important; display: flex !important; } .page-loading-messagebox { position: unset !important; } .office-form-page-padding > div { max-width: 1000px !important; width: 1000px !important; }'; document.head.appendChild(style);";
			await sender.InvokeScriptAsync("eval", new string[] { func });
		}
	}
}
