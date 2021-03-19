using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.UWP.ViewModels
{
	public class LoadingPageViewModel : ObservableObject
	{
		private string _status;
		public string Status
		{
			get => _status;
			set => SetProperty(ref _status, value);
		}
		private string _statusSub;
		public string StatusSub
		{
			get => _statusSub;
			set => SetProperty(ref _statusSub, value);
		}
		private bool _active;
		public bool Active
		{
			get => _active;
			set => SetProperty(ref _active, value);
		}
		private bool _updating;
		public bool Updating
		{
			get => _updating;
			set => SetProperty(ref _updating, value);
		}
		private double _progress;
		public double Progress
		{
			get => _progress;
			set => SetProperty(ref _progress, value);
		}
	}
}
