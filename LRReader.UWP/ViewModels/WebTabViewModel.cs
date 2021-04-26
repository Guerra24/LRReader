using LRReader.Internal;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.ViewModels
{
	public class WebTabViewModel : ObservableObject
	{
		private static List<string> Allowed = new List<string>() { "/upload", "/batch", "/config", "/config/plugins", "/logs" };

		private string _title = "Web Content";
		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}
		private string _error = "";
		public string Error
		{
			get => _error;
			set => SetProperty(ref _error, value);
		}
		private bool _showError;
		public bool ShowError
		{
			get => _showError;
			set => SetProperty(ref _showError, value);
		}
		public Uri Page;

		public string TabId;

		private bool _redirect;

		public void NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
		{
			string path = args.Uri.AbsolutePath;
			if (path.Equals("/login") || Allowed.Contains(path))
			{
				_redirect = true;
			}
			else if (path.Equals("/"))
			{
				args.Cancel = true;
				Service.Events.CloseTabWithId(TabId);
			}
			else if (path.Equals(Page.AbsolutePath))
			{
				Title = "Loading...";
			}
			else
			{
				if (_redirect)
				{
					_redirect = false;
					sender.Navigate(Page);
				}
				else
				{
					args.Cancel = true;
				}
			}
		}

		public void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
		{
			if (args.IsSuccess)
			{
				Title = sender.DocumentTitle;
			}
			else
			{
				Error = "Page \"" + args.Uri.ToString() + "\" failed to load with error \"" + args.WebErrorStatus.ToString() + "\"";
				ShowError = true;
			}
		}
	}
}
