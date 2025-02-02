using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;

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

		private CancellationTokenSource Cts = new();

		public ArchiveItemViewModel(SettingsService settings, ArchivesService archives, ApiService api, PlatformService platform, TabsService tabs, ImageProcessingService imageProcessing) : base(settings, archives, api, platform, tabs)
		{
			ImageProcessing = imageProcessing;
		}

		public async Task Phase0()
		{
			await Hide.InvokeAsync(false);
		}

		public void Phase1(Archive archive)
		{
			Archive = archive;
			MissingImage = false;
		}

		public async Task Phase2(int decodePixelWidth = 0, int decodePixelHeight = 0)
		{
			Cts.Cancel();
			Cts.Dispose();
			Cts = new();
			var token = Cts.Token;
			await Hide.InvokeAsync(false);

			var img = await Service.Images.GetThumbnailCached(Archive.arcid, cancellationToken: token);

			if (token.IsCancellationRequested)
				return;

			Thumbnail = await ImageProcessing.ByteToBitmap(img, decodePixelWidth, decodePixelHeight, image: Thumbnail, cancellationToken: token);

			if (token.IsCancellationRequested)
				return;

			if (Thumbnail == null)
				MissingImage = true;
			await Show.InvokeAsync(Platform.AnimationsEnabled);
		}

		public async Task Load(Archive archive, int decodePixelWidth = 0, int decodePixelHeight = 0)
		{
			if (!Archive.Equals(archive))
			{
				await Hide.InvokeAsync(false);

				Archive = archive;
				//await LoadArchive();

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
