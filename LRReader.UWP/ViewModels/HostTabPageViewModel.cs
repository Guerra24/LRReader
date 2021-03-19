using LRReader.Internal;
using LRReader.Shared.Internal;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.UWP.ViewModels
{
	public class HostTabPageViewModel : ObservableObject
	{
		private CustomTab _currentTab;
		public CustomTab CurrentTab
		{
			get => _currentTab;
			set => SetProperty(ref _currentTab, value);
		}
		private bool _fullscreen = false;
		public bool FullScreen
		{
			get => _fullscreen;
			set
			{
				SetProperty(ref _fullscreen, value);
				OnPropertyChanged("Windowed");
			}
		}
		public bool Windowed
		{
			get => !_fullscreen;
		}
		public ControlFlags ControlFlags
		{
			get => SharedGlobal.ControlFlags;
		}

	}
}
