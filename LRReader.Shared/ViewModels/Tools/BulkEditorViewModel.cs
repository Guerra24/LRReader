using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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

		[ICommand]
		public async Task DeleteArchives(IList<object> archives)
		{
			if (archives.Count == 0)
				return;
			foreach (var a in archives.ToList().Cast<Archive>())
				await Archives.DeleteArchive(a.arcid);
		}

		[ICommand]
		public async Task ChangeCategory(IList<object> archives)
		{
			if (archives.Count == 0)
				return;
			var items = archives.ToList().Cast<Archive>();
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
