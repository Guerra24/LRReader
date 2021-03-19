using LRReader.Internal;
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
			set
			{
				_title = value;
				OnPropertyChanged("Title");
			}
		}
		private string _error = "";
		public string Error
		{
			get => _error;
			set
			{
				_error = value;
				OnPropertyChanged("Error");
			}
		}
		private bool _showError;
		public bool ShowError
		{
			get => _showError;
			set
			{
				_showError = value;
				OnPropertyChanged("ShowError");
			}
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
				Global.EventManager.CloseTabWithHeader(TabId);
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
