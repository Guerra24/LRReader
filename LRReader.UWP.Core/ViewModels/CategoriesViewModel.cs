using GalaSoft.MvvmLight;
using static LRReader.Shared.Providers.Providers;
using static LRReader.Internal.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Internal;
using LRReader.UWP.Views.Tabs;
using GalaSoft.MvvmLight.Threading;

namespace LRReader.UWP.ViewModels
{
	public class CategoriesViewModel : ViewModelBase
	{
		private bool _loadingCategories = true;
		public bool LoadingCategories
		{
			get => _loadingCategories;
			set
			{
				_loadingCategories = value;
				RaisePropertyChanged("LoadingCategories");
				RaisePropertyChanged("ControlsEnabled");
			}
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				_refreshOnErrorButton = value;
				RaisePropertyChanged("RefreshOnErrorButton");
				RaisePropertyChanged("ControlsEnabled");
			}
		}
		public ObservableCollection<Category> CategoriesList = new ObservableCollection<Category>();
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set
			{
				_controlsEnabled = value;
				RaisePropertyChanged("ControlsEnabled");
			}
		}
		protected bool _internalLoadingCategories;

		public async Task Refresh()
		{
			if (_internalLoadingCategories)
				return;
			ControlsEnabled = false;
			_internalLoadingCategories = true;
			RefreshOnErrorButton = false;
			LoadingCategories = true;
			CategoriesList.Clear();
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				await Task.Run(async () =>
				{
					foreach (var a in result)
					{
						await DispatcherHelper.RunAsync(() => CategoriesList.Add(a));
					}
				});
			}
			else
				RefreshOnErrorButton = true;
			LoadingCategories = false;
			_internalLoadingCategories = false;
			ControlsEnabled = true;
		}

	}
}
