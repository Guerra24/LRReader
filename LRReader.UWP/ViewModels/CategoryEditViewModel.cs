using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class CategoryEditViewModel : ObservableObject
	{
		private readonly ArchivesService Archives;

		public Category category;

		public string Name { get; set; }
		public string Search { get; set; }
		public bool Pinned { get; set; }

		private bool _canSave;
		public bool CanSave
		{
			get => _canSave;
			set => SetProperty(ref _canSave, value);
		}

		public ObservableCollection<Archive> CategoryArchives = new ObservableCollection<Archive>();

		public bool Empty => CategoryArchives.Count == 0;

		private bool _loading;

		public CategoryEditViewModel(ArchivesService archives)
		{
			Archives = archives;
		}

		public async Task LoadCategory(Category cat)
		{
			if (_loading)
				return;
			_loading = true;
			category = cat;
			Name = cat.name;
			Search = cat.search;
			Pinned = cat.pinned;
			OnPropertyChanged("Name");
			OnPropertyChanged("Search");
			OnPropertyChanged("Pinned");

			var removeMissing = new List<string>();
			foreach (var a in category.archives)
			{
				var archive = Archives.GetArchive(a);
				if (archive != null)
					CategoryArchives.Add(archive);
				else
				{
					removeMissing.Add(a);
					await CategoriesProvider.RemoveArchiveFromCategory(category.id, a);
				}
			}
			OnPropertyChanged("Empty");
			removeMissing.ForEach(a => category.archives.Remove(a));
			_loading = false;
		}

		public async Task Refresh()
		{
			if (_loading)
				return;
			_loading = true;
			CategoryArchives.Clear();
			if (SharedGlobal.ControlFlags.V077)
			{
				var result = await CategoriesProvider.GetCategory(category.id);
				if (result == null)
					return;
				category.name = result.name;
				category.last_used = result.last_used;
				category.pinned = result.pinned;
				category.search = result.search;
				category.archives = result.archives;
			}
			else
			{
				var result = await CategoriesProvider.GetCategories();
				if (result == null)
					return;
				var tmp = result.FirstOrDefault(c => c.id.Equals(category.id));
				category.name = tmp.name;
				category.last_used = tmp.last_used;
				category.pinned = tmp.pinned;
				category.search = tmp.search;
				category.archives = tmp.archives;
			}
			Name = category.name;
			Search = category.search;
			Pinned = category.pinned;
			OnPropertyChanged("Name");
			OnPropertyChanged("Search");
			OnPropertyChanged("Pinned");

			var removeMissing = new List<string>();
			foreach (var a in category.archives)
			{
				var archive = Archives.GetArchive(a);
				if (archive != null)
					CategoryArchives.Add(archive);
				else
				{
					removeMissing.Add(a);
					await CategoriesProvider.RemoveArchiveFromCategory(category.id, a);
				}
			}
			removeMissing.ForEach(a => category.archives.Remove(a));

			foreach (var a in category.archives)
			{
				var archive = Archives.GetArchive(a);
				CategoryArchives.Add(archive);
			}
			OnPropertyChanged("Empty");
			_loading = false;
		}

		public async Task SaveCategory()
		{
			var result = await CategoriesProvider.UpdateCategory(category.id, Name, Search, Pinned);
			if (result)
			{
				category.name = Name;
				category.search = Search;
				category.pinned = Pinned;
				OnPropertyChanged("Name");
			}
		}

		public async Task AddToCategory(string archiveID)
		{
			if (category.archives.Contains(archiveID))
				return;
			var result = await CategoriesProvider.AddArchiveToCategory(category.id, archiveID);
			if (result)
			{
				category.archives.Add(archiveID);
				CategoryArchives.Add(Archives.GetArchive(archiveID));
				OnPropertyChanged("Empty");
			}
		}

		public async Task RemoveFromCategory(string archiveID)
		{
			var result = await CategoriesProvider.RemoveArchiveFromCategory(category.id, archiveID);
			if (result)
			{
				category.archives.Remove(archiveID);
				CategoryArchives.Remove(Archives.GetArchive(archiveID));
				OnPropertyChanged("Empty");
			}
		}
	}
}
