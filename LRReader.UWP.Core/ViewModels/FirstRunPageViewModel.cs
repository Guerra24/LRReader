using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class FirstRunPageViewModel : ViewModelBase
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
		public SettingsManager SettingsManager
		{
			get => Global.SettingsManager;
		}
	}
}
