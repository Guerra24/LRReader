using System.Threading.Tasks;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.Shared.ViewModels.Items
{
	public partial class ArchiveItemViewModel : ArchiveBaseViewModel
	{
		private readonly ImageProcessingService ImageProcessing;

		[ObservableProperty]
		private bool _missingImage;
		[ObservableProperty]
		private object? _thumbnail;

		public event AsyncAction<bool>? Show;
		public event AsyncAction<bool>? Hide;

		public ArchiveItemViewModel(SettingsService settings, ArchivesService archives, ApiService api, PlatformService platform, TabsService tabs, ImageProcessingService imageProcessing) : base(settings, archives, api, platform, tabs)
		{
			ImageProcessing = imageProcessing;
		}

		public async Task Load(Archive archive, int decodePixelWidth = 0, int decodePixelHeight = 0)
		{
			if (!Archive.Equals(archive))
			{
				Archive = archive;
				//await LoadArchive();

				await Hide.InvokeAsync(false);

				MissingImage = false;

				Thumbnail = await ImageProcessing.ByteToBitmap(await Service.Images.GetThumbnailCached(Archive.arcid), decodePixelWidth, decodePixelHeight, image: Thumbnail);

				if (Thumbnail != null)
					await Show.InvokeAsync(Platform.AnimationsEnabled);
				else
					MissingImage = true;
			}
		}
	}
}
