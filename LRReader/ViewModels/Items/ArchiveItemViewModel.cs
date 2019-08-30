using GalaSoft.MvvmLight;
using LRReader.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.ViewModels.Items
{
	public class ArchiveItemViewModel : ViewModelBase
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
		private Archive _archive = new Archive() { arcid = "", isnew = "" };
		public Archive Archive
		{
			get => _archive;
			set
			{
				if (_archive != value)
				{
					if (!_archive.arcid.Equals(value.arcid))
					{
						_archive = value;
						RaisePropertyChanged("Archive");
						RaisePropertyChanged("IsNew");
					}
				}
			}
		}
		public bool IsNew
		{
			get => _archive != null ? _archive.isnew.Equals("block") : false;
		}
	}
}
