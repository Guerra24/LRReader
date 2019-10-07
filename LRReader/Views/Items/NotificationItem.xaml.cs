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

namespace LRReader.Views.Items
{
	public sealed partial class NotificationItem : UserControl
	{

		private string title;
		private string content;
		private bool showContent;

		public NotificationItem(string title, string content)
		{
			this.title = title;
			this.content = content;
			this.showContent = !string.IsNullOrEmpty(content);
			this.InitializeComponent();
		}
	}
}
