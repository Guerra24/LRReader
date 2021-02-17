using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.UWP.Views.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels.Base
{
	public class CategoryBaseViewModel : ViewModelBase
	{
		private Category _category;
		public Category Category
		{
			get => _category;
			set
			{
				_category = value;
				RaisePropertyChanged("Category");
			}
		}
		private bool _missingImage = false;
		public bool MissingImage
		{
			get => _missingImage;
			set
			{
				_missingImage = value;
				RaisePropertyChanged("MissingImage");
			}
		}
		private bool _searchImage = false;
		public bool SearchImage
		{
			get => _searchImage;
			set
			{
				_searchImage = value;
				RaisePropertyChanged("SearchImage");
			}
		}
		public bool CanEdit => Global.SettingsManager.Profile.HasApiKey;

		public async Task UpdateCategory(string name, string search, bool pinned)
		{
			var result = await CategoriesProvider.UpdateCategory(Category.id, name, search, pinned);
			if (result)
			{
				Category.name = name;
				Category.search = search;
				Category.pinned = pinned;
				RaisePropertyChanged("Category");
			}
		}
	}
}
