using LRReader.Shared.Services;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Dialogs
{
	public sealed partial class MarkdownDialog : ContentDialog
	{
		private string text;

		public MarkdownDialog(string title, string text)
		{
			this.InitializeComponent();
			this.Title = title;
			this.text = text;
		}

		private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
			{
				await Service.Platform.OpenInBrowser(link);
			}
		}

	}
}
