using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace LRReader.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		private bool _isLoading = false;
		public bool IsLoading
		{
			get
			{
				return _isLoading;
			}
			set
			{
				_isLoading = value;
				RaisePropertyChanged("IsLoading");
			}
		}
		private string _title;
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if (value != _title)
				{
					_title = value;
					RaisePropertyChanged("Title");
				}
			}
		}
		public MainPageViewModel()
		{
		}
	}
}
