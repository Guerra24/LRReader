using GalaSoft.MvvmLight;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public void LoadArchive(Archive archive)
		{
			Archive = archive;
			Title = archive.title;
			Tags = archive.tags;
			RaisePropertyChanged("Title");
			RaisePropertyChanged("Tags");
			RaisePropertyChanged("Archive");
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
	}
}
