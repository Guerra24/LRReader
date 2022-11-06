#nullable enable
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class NotificationItem : UserControl
	{

		private string title;
		private string content;
		private bool showContent;

		public NotificationItem(string title, string? content)
		{
			this.title = title;
			this.content = content ?? "";
			this.showContent = !string.IsNullOrEmpty(content);
			this.InitializeComponent();
		}
	}
}
