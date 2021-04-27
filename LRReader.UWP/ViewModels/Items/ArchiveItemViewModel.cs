using LRReader.Shared.Services;
using LRReader.UWP.ViewModels.Base;

namespace LRReader.UWP.ViewModels.Items
{
	public class ArchiveItemViewModel : ArchiveBaseViewModel
	{
		private bool _missingImage = false;
		public bool MissingImage
		{
			get => _missingImage;
			set => SetProperty(ref _missingImage, value);
		}

		public ArchiveItemViewModel(SettingsService settings, ArchivesService archives) : base(settings, archives)
		{
		}
	}
}
