using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;

namespace LRReader.Shared.ViewModels
{
	public partial class TankoubonEditViewModel : TankoubonBaseViewModel
	{
		private readonly ArchivesService Archives;
		private readonly ApiService Api;

		[ObservableProperty]
		private bool _canSave;

		public ObservableCollection<Archive> TankoubonArchives = new ObservableCollection<Archive>();

		public bool Empty => TankoubonArchives.Count == 0;

		private bool _loading;

		public TankoubonEditViewModel(PlatformService platform, TabsService tabs, SettingsService settings, ArchivesService archives, ApiService api): base(platform, tabs, settings)
		{
			Archives = archives;
			Api = api;
		}

		public async Task Load(Tankoubon tankoubon)
		{
			if (_loading)
				return;
			Tankoubon = tankoubon;
			await Refresh();
		}

		public async Task Refresh()
		{
			if (_loading)
				return;
			_loading = true;
			TankoubonArchives.Clear();

			var result = await TankoubonsProvider.GetTankoubon(Tankoubon.id);
			if (result == null)
				return;
			Tankoubon.name = result.result.name;
			Tankoubon.archives = result.result.archives;

			var removeMissing = new List<string>();
			foreach (var a in Tankoubon.archives)
			{
				var archive = await Archives.GetOrAddArchive(a);
				if (archive != null)
					TankoubonArchives.Add(archive);
				else
				{
					removeMissing.Add(a);
					await TankoubonsProvider.RemoveArchive(Tankoubon.id, a);
				}
			}
			removeMissing.ForEach(a => Tankoubon.archives.Remove(a));
			OnPropertyChanged("Empty");
			_loading = false;
		}

		public async Task Save()
		{
			/*var result = await TankoubonsProvider.U(category.id, Name, Search, Pinned);
			if (result)
			{
				category.name = Name;
				category.search = Search;
				category.pinned = Pinned;
				OnPropertyChanged("Name");
			}*/
		}

		public async Task AddToTankoubon(string archiveID)
		{
			if (Tankoubon.archives.Contains(archiveID))
				return;
			var result = await TankoubonsProvider.AddArchive(Tankoubon.id, archiveID);
			if (result)
			{
				var item = Archives.GetArchive(archiveID);
				if (item != null)
				{
					Tankoubon.archives.Add(archiveID);
					TankoubonArchives.Add(item);
					OnPropertyChanged("Empty");
				}
			}
		}

		public async Task RemoveFromTankoubon(string archiveID)
		{
			var result = await TankoubonsProvider.RemoveArchive(Tankoubon.id, archiveID);
			if (result)
			{
				var item = Archives.GetArchive(archiveID);
				if (item != null)
				{
					Tankoubon.archives.Remove(archiveID);
					TankoubonArchives.Remove(item);
					OnPropertyChanged("Empty");
				}
			}
		}
	}
}
