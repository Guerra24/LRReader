using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public class ArchiveEditViewModel : ObservableObject
	{
		private readonly EventsService Events;

		public Archive Archive;

		public string Title { get; set; }
		public string Tags { get; set; }

		private bool _saving;
		public bool Saving
		{
			get => _saving;
			set => SetProperty(ref _saving, value);
		}

		public ObservableCollection<Plugin> Plugins = new ObservableCollection<Plugin>();

		public ObservableCollection<EditableTag> TagsList = new ObservableCollection<EditableTag>();

		private Plugin _currentPlugin;

		public Plugin CurrentPlugin
		{
			get => _currentPlugin;
			set => SetProperty(ref _currentPlugin, value);
		}

		public string Arg = "";

		public ArchiveEditViewModel(EventsService events)
		{
			Events = events;
		}

		public async Task LoadArchive(Archive archive)
		{
			Archive = archive;
			Title = archive.title;
			Tags = archive.tags;
			OnPropertyChanged("Title");
			OnPropertyChanged("Tags");
			OnPropertyChanged("Archive");
			TagsList.Clear();
			foreach (var t in Tags.Split(','))
				TagsList.Add(new EditableTag { Tag = t.Trim() });
			TagsList.Add(new AddTag());
			var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
			Plugins.Clear();
			plugins.ForEach(p => Plugins.Add(p));
			CurrentPlugin = Plugins.ElementAt(0);
		}

		public async Task ReloadArchive()
		{
			var result = await ArchivesProvider.GetArchive(Archive.arcid);
			if (result != null)
			{
				Title = result.title;
				Tags = result.tags;
				TagsList.Clear();
				foreach (var t in Tags.Split(','))
					TagsList.Add(new EditableTag { Tag = t.Trim() });
				TagsList.Add(new AddTag());
				OnPropertyChanged("Title");
				OnPropertyChanged("Tags");
			}
			var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
			Plugins.Clear();
			plugins.ForEach(p => Plugins.Add(p));
			CurrentPlugin = Plugins.ElementAt(0);
		}

		public async Task SaveArchive()
		{
			Saving = true;
			var result = await ArchivesProvider.UpdateArchive(Archive.arcid, Title, Tags);
			if (result)
			{
				Archive.title = Title;
				Archive.tags = Tags;
				TagsList.Clear();
				foreach (var t in Tags.Split(','))
					TagsList.Add(new EditableTag { Tag = t.Trim() });
				TagsList.Add(new AddTag());
				OnPropertyChanged("Archive");
			}
			Saving = false;
		}

		public async Task UsePlugin()
		{
			await SaveArchive();
			Saving = true;
			var result = await ServerProvider.UsePlugin(CurrentPlugin.@namespace, Archive.arcid, Arg);
			if (result != null)
			{
				if (result.success)
				{
					if (!string.IsNullOrEmpty(result.data.new_tags))
					{
						if (!Tags.TrimEnd().EndsWith(","))
						{
							Tags = Tags.TrimEnd() + ",";
						}
						Tags += result.data.new_tags;
						OnPropertyChanged("Tags");
					}
				}
				else
				{
					Events.ShowNotification("Error while fetching tags", result.error, 0);
				}
			}
			Saving = false;
		}

		public void AddEmptyTag()
		{
			TagsList.Insert(TagsList.Count - 1, new EditableTag { Tag = "" });
		}

		public void RemoveTag(EditableTag editableTag)
		{
			TagsList.Remove(editableTag);
		}

	}

	public class EditableTag
	{
		public string Tag { get; set; }
	}

	public class AddTag : EditableTag
	{

	}
}
