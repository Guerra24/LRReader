using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;

namespace LRReader.Shared.ViewModels.Items
{
	public class ArchiveItemViewModel : ArchiveBaseViewModel
	{
		private bool _missingImage = false;
		public bool MissingImage
		{
			get => _missingImage;
			set => SetProperty(ref _missingImage, value);
		}

		public ArchiveItemViewModel(SettingsService settings, ArchivesService archives, ApiService api, IPlatformService platform) : base(settings, archives, api, platform)
		{
		}
	}
}
