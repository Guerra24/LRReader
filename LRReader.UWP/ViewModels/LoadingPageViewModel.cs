using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.UWP.ViewModels
{
	public class LoadingPageViewModel : ObservableObject
	{
		private string _status;
		public string Status
		{
			get => _status;
			set
			{
				_status = value;
				OnPropertyChanged("Status");
			}
		}
		private string _statusSub;
		public string StatusSub
		{
			get => _statusSub;
			set
			{
				_statusSub = value;
				OnPropertyChanged("StatusSub");
			}
		}
		private bool _active;
		public bool Active
		{
			get => _active;
			set
			{
				_active = value;
				OnPropertyChanged("Active");
			}
		}
		private bool _updating;
		public bool Updating
		{
			get => _updating;
			set
			{
				_updating = value;
				OnPropertyChanged("Updating");
			}
		}
		private double _progress;
		public double Progress
		{
			get => _progress;
			set
			{
				_progress = value;
				OnPropertyChanged("Progress");
			}
		}
	}
}
