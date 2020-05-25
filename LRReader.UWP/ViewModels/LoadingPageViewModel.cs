using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class LoadingPageViewModel : ViewModelBase
	{
		private string _status;
		public string Status
		{
			get => _status;
			set
			{
				_status = value;
				RaisePropertyChanged("Status");
			}
		}
		private string _statusSub;
		public string StatusSub
		{
			get => _statusSub;
			set
			{
				_statusSub = value;
				RaisePropertyChanged("StatusSub");
			}
		}
		private bool _active;
		public bool Active
		{
			get => _active;
			set
			{
				_active = value;
				RaisePropertyChanged("Active");
			}
		}
	}
}
