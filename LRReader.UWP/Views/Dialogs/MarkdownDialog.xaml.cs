using LRReader.Shared.Models;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class MarkdownDialog : ContentDialog, IDialog
	{

		public MarkdownDialog(string title, string text)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
			this.Title = title;
			WebView.SetMarkdown(text);
		}

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();
	}
}
