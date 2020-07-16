using GalaSoft.MvvmLight;
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
using GalaSoft.MvvmLight.Command;
using LRReader.Shared.Providers;

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

		public async Task CreateCategory(string name, string search = "", bool pinned = false)
		{
			var resultCreate = await CategoriesProvider.CreateCategory(name, search, pinned);
			if (resultCreate != null)
			{
				resultCreate.DeleteCategory += DeleteCategory;
				CategoriesList.Add(resultCreate);
			}
		}

		public async Task DeleteCategory(Category category)
		{
			var result = await CategoriesProvider.DeleteCategory(category.id);
			if (result)
			{
				CategoriesList.Remove(category);
			}
		}

		public async Task Refresh()
		{
			if (_internalLoadingCategories)
				return;
			ControlsEnabled = false;
			_internalLoadingCategories = true;
			RefreshOnErrorButton = false;
			LoadingCategories = true;
			CategoriesList.Clear();
			//if (SharedGlobal.SettingsManager.Profile.HasApiKey)
				//CategoriesList.Add(new AddNewCategory());
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				await Task.Run(async () =>
				{
					foreach (var a in result.OrderBy(c => !c.pinned))
					{
						a.DeleteCategory += DeleteCategory;
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
