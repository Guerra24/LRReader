using GalaSoft.MvvmLight;
using static LRReader.Internal.Global;
using LRReader.Shared.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Providers;

namespace LRReader.UWP.ViewModels
{
	public class CategoryEditViewModel : ViewModelBase
	{
		public Category Category;

		public string Name { get; set; }
		public string Search { get; set; }
		public bool Pinned { get; set; }

		private bool _canSave;
		public bool CanSave
		{
			get => _canSave;
			set
			{
				_canSave = value;
				RaisePropertyChanged("CanSave");
			}
		}

		public void LoadCategory(Category category)
		{
			Category = category;
			Name = category.name;
			Search = category.search;
			Pinned = category.pinned;
			RaisePropertyChanged("Name");
			RaisePropertyChanged("Search");
			RaisePropertyChanged("Pinned");
		}

		public async Task SaveCategory()
		{
			var result = await CategoriesProvider.UpdateCategory(Category.id, Category.name, Category.search, Category.pinned);
			if (result)
			{
				Category.name = Name;
				Category.search = Search;
				Category.pinned = Pinned;
				RaisePropertyChanged("Name");
			}
		}
	}
}
