using GalaSoft.MvvmLight;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class CategoryArchiveViewModel : ViewModelBase
	{
		public string archiveID;

		public ObservableCollection<Category> Categories = new ObservableCollection<Category>();
		private List<Category> Source = new List<Category>();
		public IList<object> SelectedCategories;

		public CategoryArchiveViewModel(string archiveID)
		{
			this.archiveID = archiveID;
		}

		public async Task Load()
		{
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				foreach (var c in result)
					if (string.IsNullOrEmpty(c.search))
					{
						Categories.Add(c);
						Source.Add(c);
						if (c.archives.Contains(archiveID))
							SelectedCategories.Add(c);
					}
			}
		}

		public async Task Reload()
		{
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				foreach (var c in result)
					if (string.IsNullOrEmpty(c.search))
					{
						var target = Source.FirstOrDefault(cat => cat.id.Equals(c.id));
						if (target != null)
							target.archives = c.archives;
					}
			}
		}

		public async Task<bool> AddToCategory(string id)
		{
			System.Diagnostics.Debug.WriteLine($"Added to {id}");
			return await CategoriesProvider.AddArchiveToCategory(id, archiveID);
		}

		public async Task<bool> RemoveFromCategory(string id)
		{
			System.Diagnostics.Debug.WriteLine($"Removed from {id}");
			return await CategoriesProvider.RemoveArchiveFromCategory(id, archiveID);
		}

		public void Search(string text)
		{
			List<Category> temp = Source.Where(c => c.name.Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

			for (int i = Categories.Count - 1; i >= 0; i--)
			{
				var item = Categories[i];
				if (!temp.Contains(item))
				{
					Categories.Remove(item);
				}
			}

			foreach (var item in temp)
			{
				if (!Categories.Contains(item))
				{
					Categories.Add(item);
				}
			}

			foreach (var c in Categories)
				if (c.archives.Contains(archiveID))
					SelectedCategories.Add(c);
		}
	}
}
