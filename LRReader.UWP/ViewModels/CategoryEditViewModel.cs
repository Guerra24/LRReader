﻿using GalaSoft.MvvmLight;
using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class CategoryEditViewModel : ViewModelBase
	{
		public Category category;

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

		private bool _loading;

		public void LoadCategory(Category cat)
		{
			category = cat;
			Name = cat.name;
			Search = cat.search;
			Pinned = cat.pinned;
			RaisePropertyChanged("Name");
			RaisePropertyChanged("Search");
			RaisePropertyChanged("Pinned");

			foreach (var a in cat.archives)
			{
				var archive = SharedGlobal.ArchivesManager.GetArchive(a);
				CategoryArchives.Add(archive);
			}
		}

		public async Task Refresh()
		{
			if (_loading)
				return;
			_loading = true;
			CategoryArchives.Clear();
			if (SharedGlobal.ControlFlags.CategoriesV2)
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
			RaisePropertyChanged("Name");
			RaisePropertyChanged("Search");
			RaisePropertyChanged("Pinned");

			foreach (var a in category.archives)
			{
				var archive = SharedGlobal.ArchivesManager.GetArchive(a);
				CategoryArchives.Add(archive);
			}
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
				RaisePropertyChanged("Name");
			}
		}

		public async Task AddToCategory(string archiveID)
		{
			if (category.archives.Contains(archiveID))
				return;
			System.Diagnostics.Debug.WriteLine($"Added to {category.id}");
			var result = await CategoriesProvider.AddArchiveToCategory(category.id, archiveID);
			if (result)
			{
				category.archives.Add(archiveID);
				CategoryArchives.Add(SharedGlobal.ArchivesManager.GetArchive(archiveID));
			}
		}

		public async Task RemoveFromCategory(string archiveID)
		{
			System.Diagnostics.Debug.WriteLine($"Removed from {category.id}");
			var result = await CategoriesProvider.RemoveArchiveFromCategory(category.id, archiveID);
			if (result)
			{
				category.archives.Remove(archiveID);
				CategoryArchives.Remove(SharedGlobal.ArchivesManager.GetArchive(archiveID));
			}
		}
	}
}
