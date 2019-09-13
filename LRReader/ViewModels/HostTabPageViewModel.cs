using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.ViewModels
{
	public class HostTabPageViewModel : ViewModelBase
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

		public ObservableCollection<TabViewItem> Tabs = new ObservableCollection<TabViewItem>();

		private TabViewItem _currentTab;
		public TabViewItem CurrentTab
		{
			get => _currentTab;
			set
			{
				if (_currentTab != value)
				{
					_currentTab = value;
					RaisePropertyChanged("CurrentTab");
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
					RaisePropertyChanged("FullScreen");
					RaisePropertyChanged("Windowed");
				}
			}
		}
		public bool Windowed
		{
			get => !_fullscreen;
		}

		public HostTabPageViewModel()
		{
			Global.EventManager.AddTabEvent += AddTab;
			Global.EventManager.CloseAllTabsEvent += CloseAllTabs;
		}

		public async void AddTab(TabViewItem tab)
		{
			var current = Tabs.FirstOrDefault(t => t.Header.Equals(tab.Header));
			if (current != null)
			{
				CurrentTab = current;
			}
			else
			{
				Tabs.Add(tab);
				await DispatcherHelper.RunAsync(() => CurrentTab = tab);
			}
		}

		public void CloseAllTabs()
		{
			Tabs.Clear();
		}

	}
}
