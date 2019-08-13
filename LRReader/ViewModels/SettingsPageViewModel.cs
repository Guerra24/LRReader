using GalaSoft.MvvmLight;
using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.ViewModels
{
	public class SettingsPageViewModel : ViewModelBase
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
		public SettingsManager SettingsManager
		{
			get => Global.SettingsManager;
		}
		public string Version
		{
			get => Util.GetAppVersion();
		}
	}
}
