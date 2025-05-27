using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Tools
{
	public partial class BulkEditorViewModel : ObservableObject
	{
		private readonly ArchivesService Archives;

		public ObservableCollection<Category> Categories = new ObservableCollection<Category>();

		[ObservableProperty]
		private Category? _selectedCategory;

		[ObservableProperty]
		private bool _moveToCategory;

		public BulkEditorViewModel(ArchivesService archives)
		{
			Archives = archives;
		}

		public async Task Load()
		{
			var id = SelectedCategory?.id;
			Categories.Clear();
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				foreach (var c in result)
					if (string.IsNullOrEmpty(c.search))
						Categories.Add(c);
				SelectedCategory = Categories.FirstOrDefault(c => c.id.Equals(id));
				if (SelectedCategory == null)
					SelectedCategory = Categories.FirstOrDefault();
			}
		}

		[RelayCommand]
		public async Task DeleteArchives(IList<object> selected)
		{
			var items = selected.ToList().Cast<Archive>().ToList();
			if (items.Count == 0)
				return;
			foreach (var a in items)
				await Archives.DeleteArchive(a.arcid);
		}

		[RelayCommand]
		public async Task ChangeCategory(IList<object> selected)
		{
			var items = selected.ToList().Cast<Archive>().ToList();
			if (items.Count == 0)
				return;
			if (MoveToCategory)
			{
				foreach (var a in items)
					foreach (var c in Categories)
						if (c.archives.Contains(a.arcid))
							await CategoriesProvider.RemoveArchiveFromCategory(c.id, a.arcid);
			}
			foreach (var a in items)
				await CategoriesProvider.AddArchiveToCategory(SelectedCategory?.id ?? "", a.arcid);
			MoveToCategory = false;
		}
	}
}
