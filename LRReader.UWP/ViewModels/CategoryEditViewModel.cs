using GalaSoft.MvvmLight;
using static LRReader.Internal.Global;
using LRReader.Shared.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRReader.Shared.Providers;
using System.Collections.ObjectModel;
using LRReader.Shared.Internal;

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

		public ObservableCollection<Archive> CategoryArchives = new ObservableCollection<Archive>();

		public async Task LoadCategory(string id)
		{
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				Category = result.FirstOrDefault(c => c.id.Equals(id));
				Name = Category.name;
				Search = Category.search;
				Pinned = Category.pinned;
				RaisePropertyChanged("Name");
				RaisePropertyChanged("Search");
				RaisePropertyChanged("Pinned");

				foreach (var a in Category.archives)
				{
					var archive = SharedGlobal.ArchivesManager.Archives.FirstOrDefault(b => b.arcid == a);
					CategoryArchives.Add(archive);
				}
			}
		}

		public async Task SaveCategory()
		{
			var result = await CategoriesProvider.UpdateCategory(Category.id, Name, Search, Pinned);
			if (result)
			{
				Category.name = Name;
				Category.search = Search;
				Category.pinned = Pinned;
				RaisePropertyChanged("Name");
			}
		}

		public async Task AddToCategory(string archiveID)
		{
			if (Category.archives.Contains(archiveID))
				return;
			System.Diagnostics.Debug.WriteLine($"Added to {Category.id}");
			var result = await CategoriesProvider.AddArchiveToCategory(Category.id, archiveID);
			if (result)
			{
				Category.archives.Add(archiveID);
				CategoryArchives.Add(SharedGlobal.ArchivesManager.Archives.FirstOrDefault(b => b.arcid == archiveID));
			}
		}

		public async Task RemoveFromCategory(string archiveID)
		{
			System.Diagnostics.Debug.WriteLine($"Removed from {Category.id}");
			var result = await CategoriesProvider.RemoveArchiveFromCategory(Category.id, archiveID);
			if (result)
			{
				Category.archives.Remove(archiveID);
				CategoryArchives.Remove(SharedGlobal.ArchivesManager.Archives.FirstOrDefault(b => b.arcid == archiveID));
			}
		}
	}
}
