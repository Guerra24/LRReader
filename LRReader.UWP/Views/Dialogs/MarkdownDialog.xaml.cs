using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class MarkdownDialog : ContentDialog
	{

		public MarkdownDialog(string title, string text)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
			this.Title = title;
			WebView.SetMarkdown(text);
		}
	}
}
