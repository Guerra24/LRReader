using LRReader.UWP.Extensions;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class MarkdownDialog : ContentDialog
	{

		public MarkdownDialog(string title, string text)
		{
			this.InitializeComponent();
			this.Title = title;
			WebView.SetMarkdownBase(text);
		}
	}
}
