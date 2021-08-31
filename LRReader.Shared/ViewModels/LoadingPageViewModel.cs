using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.Shared.ViewModels
{
	public partial class LoadingPageViewModel : ObservableObject
	{
		[ObservableProperty]
		private string _status = "";
		[ObservableProperty]
		private string _statusSub = "";
		[ObservableProperty]
		private bool _active;
		[ObservableProperty]
		private bool _updating;
		[ObservableProperty]
		private double _progress;
		[ObservableProperty]
		private bool _retry;
	}
}
