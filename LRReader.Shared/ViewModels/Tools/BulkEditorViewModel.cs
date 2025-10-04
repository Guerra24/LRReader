using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Tools;

public partial class BulkEditorViewModel : ObservableObject
{
	private readonly ArchivesService Archives;

	public ObservableCollection<Category> Categories = new ObservableCollection<Category>();
	public ObservableCollection<Plugin> Plugins = new ObservableCollection<Plugin>();

	[ObservableProperty]
	private Category? _selectedCategory;

	[ObservableProperty]
	private bool _moveToCategory;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(RunPluginCommand))]
	private Plugin? _plugin;

	[ObservableProperty]
	private bool _running;

	partial void OnRunningChanged(bool value)
	{
		Progress = 0;
		MaxItems = 0;
	}

	[ObservableProperty]
	private int _maxItems;

	[ObservableProperty]
	private int _progress;

	private bool RunPluginCommandCanExecute => Plugin != null;

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

	public async Task LoadPlugins()
	{
		var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
		if (plugins == null || plugins.Count == 0)
		{
			Plugin = null;
			return;
		}
		Plugins.Clear();
		plugins.ForEach(Plugins.Add);
	}

	[RelayCommand]
	private async Task DeleteArchives(IList<object> selected)
	{
		Running = true;
		var items = selected.ToList().Cast<Archive>().ToList();
		if (items.Count == 0)
			return;
		MaxItems = items.Count;
		foreach (var a in items)
		{
			Progress++;
			await Archives.DeleteArchive(a.arcid);
		}
		Running = false;
	}

	[RelayCommand]
	private async Task ChangeCategory(IList<object> selected)
	{
		Running = true;
		var items = selected.ToList().Cast<Archive>().ToList();
		if (items.Count == 0)
			return;
		MaxItems = items.Count;
		foreach (var a in items)
		{
			Progress++;
			if (MoveToCategory)
				foreach (var c in Categories)
					if (c.archives.Contains(a.arcid))
						await CategoriesProvider.RemoveArchiveFromCategory(c.id, a.arcid);
			await CategoriesProvider.AddArchiveToCategory(SelectedCategory?.id ?? "", a.arcid);
		}
		MoveToCategory = false;
		Running = false;
	}

	[RelayCommand]
	private async Task ClearTags(IList<object> selected)
	{
		Running = true;
		var items = selected.ToList().Cast<Archive>().ToList();
		if (items.Count == 0)
			return;
		MaxItems = items.Count;
		foreach (var a in items)
		{
			Progress++;
			await ArchivesProvider.UpdateArchive(a.arcid, tags: a.tags = "");
			a.UpdateTags();
		}
		Running = false;
	}

	[RelayCommand(CanExecute = nameof(RunPluginCommandCanExecute))]
	private async Task RunPlugin(IList<object> selected)
	{
		Running = true;
		var items = selected.ToList().Cast<Archive>().ToList();
		if (items.Count == 0)
			return;
		if (Plugin == null)
			return;
		MaxItems = items.Count;
		foreach (var a in items)
		{
			Progress++;
			var result = await ServerProvider.UsePlugin(Plugin.@namespace, a.arcid);
			if (result != null && result.success && !string.IsNullOrEmpty(result.data.new_tags))
			{
				a.tags += $", {result.data.new_tags}";
				a.BuildVirtualTags();
				a.tags = a.BuildStringTags();
				a.UpdateTags();
				await ArchivesProvider.UpdateArchive(a.arcid, tags: a.tags);
			}
		}
		Running = false;
	}
}
