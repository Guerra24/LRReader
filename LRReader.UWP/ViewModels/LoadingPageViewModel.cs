using GalaSoft.MvvmLight;

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
		private bool _updating;
		public bool Updating
		{
			get => _updating;
			set
			{
				_updating = value;
				RaisePropertyChanged("Updating");
			}
		}
		private double _progress;
		public double Progress
		{
			get => _progress;
			set
			{
				_progress = value;
				RaisePropertyChanged("Progress");
			}
		}
	}
}
