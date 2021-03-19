using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.ViewModels.Base
{
	public class CategoryBaseViewModel : ObservableObject
	{
		private Category _category;
		public Category Category
		{
			get => _category;
			set
			{
				_category = value;
				OnPropertyChanged("Category");
			}
		}
		private bool _missingImage = false;
		public bool MissingImage
		{
			get => _missingImage;
			set
			{
				_missingImage = value;
				OnPropertyChanged("MissingImage");
			}
		}
		private bool _searchImage = false;
		public bool SearchImage
		{
			get => _searchImage;
			set
			{
				_searchImage = value;
				OnPropertyChanged("SearchImage");
			}
		}
		public bool CanEdit => Settings.Profile.HasApiKey;

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
