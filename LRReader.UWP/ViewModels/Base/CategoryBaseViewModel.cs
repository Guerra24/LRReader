using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels.Base
{
	public class CategoryBaseViewModel : ObservableObject
	{
		protected readonly SettingsService Settings;

		private Category _category;
		public Category Category
		{
			get => _category;
			set => SetProperty(ref _category, value);
		}
		private bool _missingImage = false;
		public bool MissingImage
		{
			get => _missingImage;
			set => SetProperty(ref _missingImage, value);
		}
		private bool _searchImage = false;
		public bool SearchImage
		{
			get => _searchImage;
			set => SetProperty(ref _searchImage, value);
		}
		public bool CanEdit => Settings.Profile.HasApiKey;

		public CategoryBaseViewModel(SettingsService settings)
		{
			Settings = settings;
		}

		public async Task UpdateCategory(string name, string search, bool pinned)
		{
			var result = await CategoriesProvider.UpdateCategory(Category.id, name, search, pinned);
			if (result)
			{
				Category.name = name;
				Category.search = search;
				Category.pinned = pinned;
				OnPropertyChanged("Category");
			}
		}
	}
}
