using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Core.ViewModels
{
	public class WebTabViewModel : ViewModelBase
	{
		private bool _isLoading = false;
		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				RaisePropertyChanged("IsLoading");
			}
		}
		private string _title = "Web Content";
		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				RaisePropertyChanged("Title");
			}
		}
		private string _error = "";
		public string Error
		{
			get => _error;
			set
			{
				_error = value;
				RaisePropertyChanged("Error");
			}
		}
		private bool _showError;
		public bool ShowError
		{
			get => _showError;
			set
			{
				_showError = value;
				RaisePropertyChanged("ShowError");
			}
		}
		public Uri Page;

		private bool _redirect;

		public void NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
		{
			string path = args.Uri.AbsolutePath;
			if (path.Equals("/login"))
			{
				_redirect = true;
			}
			else if (args.Uri.AbsolutePath.Equals(Page.AbsolutePath))
			{
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
