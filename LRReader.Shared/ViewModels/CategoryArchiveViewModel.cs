using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public class CategoryArchiveViewModel : ObservableObject
	{
		public string archiveID;

		public ObservableCollection<Category> Categories = new ObservableCollection<Category>();
		private List<Category> Source = new List<Category>();
		[AllowNull]
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
			return await CategoriesProvider.AddArchiveToCategory(id, archiveID);
		}

		public async Task<bool> RemoveFromCategory(string id)
		{
			return await CategoriesProvider.RemoveArchiveFromCategory(id, archiveID);
		}

		public void Search(string text)
		{
			List<Category> temp = Source.Where(c => c.name.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();

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
