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
			set
			{
				if (_currentTab != value)
				{
					_currentTab = value;
					OnPropertyChanged("CurrentTab");
				}
			}
		}
		private bool _fullscreen = false;
		public bool FullScreen
		{
			get => _fullscreen;
			set
			{
				if (_fullscreen != value)
				{
					_fullscreen = value;
					OnPropertyChanged("FullScreen");
					OnPropertyChanged("Windowed");
				}
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
