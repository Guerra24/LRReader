using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;

namespace LRReader.Shared.ViewModels
{
	public class CategoryEditViewModel : ObservableObject
	{
		private readonly ArchivesService Archives;
		private readonly ApiService Api;

		public Category category = null!;
		public string Name { get; set; } = string.Empty;
		public string Search { get; set; } = null!;
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

		public CategoryEditViewModel(ArchivesService archives, ApiService api)
		{
			Archives = archives;
			Api = api;
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
				var archive = await Archives.GetOrAddArchive(a);
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

			var result = await CategoriesProvider.GetCategory(category.id);
			if (result == null)
				return;
			category.name = result.name;
			category.pinned = result.pinned;
			category.search = result.search;
			category.archives = result.archives;
			Name = category.name;
			Search = category.search;
			Pinned = category.pinned;
			OnPropertyChanged("Name");
			OnPropertyChanged("Search");
			OnPropertyChanged("Pinned");

			var removeMissing = new List<string>();
			foreach (var a in category.archives)
			{
				var archive = await Archives.GetOrAddArchive(a);
				if (archive != null)
					CategoryArchives.Add(archive);
				else
				{
					removeMissing.Add(a);
					await CategoriesProvider.RemoveArchiveFromCategory(category.id, a);
				}
			}
			removeMissing.ForEach(a => category.archives.Remove(a));
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
				var item = Archives.GetArchive(archiveID);
				if (item != null)
				{
					category.archives.Add(archiveID);
					CategoryArchives.Add(item);
					OnPropertyChanged("Empty");
				}
			}
		}

		public async Task RemoveFromCategory(string archiveID)
		{
			var result = await CategoriesProvider.RemoveArchiveFromCategory(category.id, archiveID);
			if (result)
			{
				var item = Archives.GetArchive(archiveID);
				if (item != null)
				{
					category.archives.Remove(archiveID);
					CategoryArchives.Remove(item);
					OnPropertyChanged("Empty");
				}
			}
		}
	}
}
