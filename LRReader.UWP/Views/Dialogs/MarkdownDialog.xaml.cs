using LRReader.Shared.Models;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class MarkdownDialog : ContentDialog, IDialog
	{
		private string Text;

		public MarkdownDialog(string title, string text)
		{
			this.InitializeComponent();
			RequestedTheme = Service.Platform.Theme.ToXamlTheme();
			Title = title;
			Text = text;
		}

		public new async Task<IDialogResult> ShowAsync() => (IDialogResult)(int)await base.ShowAsync();
	}
}
