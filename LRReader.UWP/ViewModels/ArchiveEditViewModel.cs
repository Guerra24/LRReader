using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class ArchiveEditViewModel : ViewModelBase
	{
		public Archive Archive;

		public string Title { get; set; }
		public string Tags { get; set; }

		private bool _saving;
		public bool Saving
		{
			get => _saving;
			set
			{
				_saving = value;
				RaisePropertyChanged("Saving");
			}
		}

		public ObservableCollection<Plugin> Plugins = new ObservableCollection<Plugin>();

		private Plugin _currentPlugin;
		public Plugin CurrentPlugin
		{
			get => _currentPlugin;
			set
			{
				if (_currentPlugin != value)
				{
					_currentPlugin = value;
					RaisePropertyChanged("CurrentPlugin");
				}
			}
		}
		public string Arg = "";

		public async Task LoadArchive(Archive archive)
		{
			Archive = archive;
			Title = archive.title;
			Tags = archive.tags;
			RaisePropertyChanged("Title");
			RaisePropertyChanged("Tags");
			RaisePropertyChanged("Archive");
			Plugins.Clear();
			var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
			foreach (var p in plugins)
				Plugins.Add(p);
			CurrentPlugin = Plugins.ElementAt(0);
		}

		public async Task ReloadArchive()
		{
			var result = await ArchivesProvider.GetArchive(Archive.arcid);
			if (result != null)
			{
				Title = result.title;
				Tags = result.tags;
				RaisePropertyChanged("Title");
				RaisePropertyChanged("Tags");
			}
			Plugins.Clear();
			var plugins = await ServerProvider.GetPlugins(PluginType.Metadata);
			foreach (var p in plugins)
				Plugins.Add(p);
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
				RaisePropertyChanged("Archive");
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
						RaisePropertyChanged("Tags");
					}
				}
				else
				{
					Global.EventManager.ShowNotification("Error while fetching tags", result.error, 0);
				}
			}
			Saving = false;
		}
	}
}
