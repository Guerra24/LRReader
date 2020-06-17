using GalaSoft.MvvmLight;
using static LRReader.Shared.Providers.Providers;
using static LRReader.Internal.Global;
using LRReader.Shared.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class CategoryEditViewModel : ViewModelBase
	{
		public Category Category;

		private string _name;
		public string Name
		{
			get => _name;
			set => _name = value;
		}
		private string _search;
		public string Search
		{
			get => _search;
			set => _search = value;
		}
		private bool _pinned;
		public bool Pinned
		{
			get => _pinned;
			set => _pinned = value;
		}

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
			}
		}
	}
}
