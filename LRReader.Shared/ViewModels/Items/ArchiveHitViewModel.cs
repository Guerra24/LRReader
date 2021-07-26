using LRReader.Shared.Models.Main;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.Shared.ViewModels.Items
{
	public class ArchiveHitViewModel : ObservableObject
	{
		private ArchiveHit _archiveHit;
		public ArchiveHit ArchiveHit
		{
			get => _archiveHit;
			set => SetProperty(ref _archiveHit, value);
		}
	}
}
