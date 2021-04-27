using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public class CategoriesViewModel : ObservableObject
	{
		private readonly SettingsService Settings;
		private readonly IDispatcherService Dispatcher;

		private bool _loadingCategories = true;
		public bool LoadingCategories
		{
			get => _loadingCategories;
			set
			{
				SetProperty(ref _loadingCategories, value);
				OnPropertyChanged("ControlsEnabled");
			}
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				SetProperty(ref _refreshOnErrorButton, value);
				OnPropertyChanged("ControlsEnabled");
			}
		}
		public ObservableCollection<Category> CategoriesList = new ObservableCollection<Category>();
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set => SetProperty(ref _controlsEnabled, value);
		}
		protected bool _internalLoadingCategories;

		public CategoriesViewModel(SettingsService settings, IDispatcherService dispatcher)
		{
			Settings = settings;
			Dispatcher = dispatcher;
		}

		public async Task<Category> CreateCategory(string name, string search = "", bool pinned = false)
		{
			var resultCreate = await CategoriesProvider.CreateCategory(name, search, pinned);
			if (resultCreate != null)
			{
				resultCreate.DeleteCategory += DeleteCategory;
				CategoriesList.Add(resultCreate);
				return resultCreate;
			}
			return null;
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
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				if (Settings.Profile.HasApiKey)
					CategoriesList.Add(new AddNewCategory());
				await Task.Run(async () =>
				{
					foreach (var a in result.OrderBy(c => !c.pinned))
					{
						a.DeleteCategory += DeleteCategory;
						await Dispatcher.RunAsync(() => CategoriesList.Add(a));
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
