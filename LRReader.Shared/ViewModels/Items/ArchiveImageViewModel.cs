using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyedSemaphores;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;

namespace LRReader.Shared.ViewModels.Items
{
	public partial class ArchiveImageViewModel : ObservableObject
	{
		private readonly PlatformService Platform;
		private readonly ImagesService Images;
		private readonly ImageProcessingService ImageProcessing;

		private bool _loading;

		[ObservableProperty]
		private ImagePageSet _set = new ImagePageSet("", "", 0);
		[ObservableProperty]
		private bool _missingImage;
		[ObservableProperty]
		private string _page = "";
		[ObservableProperty]
		private string _resolution = "";
		[ObservableProperty]
		private string _format = "";
		[ObservableProperty]
		private object? _thumbnail;

		[ObservableProperty]
		public bool _hideOverlay;
		[ObservableProperty]
		public bool _showExtraDetails;

		public event AsyncAction<bool>? Show;
		public event AsyncAction<bool>? Hide;

		private Guid _key = Guid.NewGuid();

		public ArchiveImageViewModel(PlatformService platform, ImagesService images, ImageProcessingService imageProcessing)
		{
			Platform = platform;
			Images = images;
			ImageProcessing = imageProcessing;
		}

		public async Task Phase0()
		{
			await Hide.InvokeAsync(false);
		}

		public void Phase1(ImagePageSet set)
		{
			Set = set;
			MissingImage = false;
		}

		public async Task Phase2()
		{
			if (!HideOverlay)
				Page = Set.Page.ToString();
			if (!HideOverlay && ShowExtraDetails)
			{
				Format = Set.Image!.Substring(Set.Image.LastIndexOf('.') + 1).ToUpper();
				var size = await Images.GetImageSizeCached(Set.Image);
				Resolution = $"{size.Width}x{size.Height}";
			}
		}

		public async Task Phase3()
		{
			var img = await Images.GetThumbnailCached(Set.Id, Set.Page);
			using (var key = await KeyedSemaphore.LockAsync($"PageThumb_{_key}"))
				Thumbnail = await ImageProcessing.ByteToBitmap(img, decodeHeight: 275, image: Thumbnail);

			if (Thumbnail != null)
				await Show.InvokeAsync(Platform.AnimationsEnabled);
			else
				MissingImage = true;
		}

		public async Task LoadImage(ImagePageSet set)
		{
			if (!Set.Equals(set))
			{
				Set = set;
				await Reload();
			}
		}

		[RelayCommand]
		public async Task Reload(bool forced = false)
		{
			if (_loading)
				return;
			_loading = true;

			await Hide.InvokeAsync(forced && Platform.AnimationsEnabled);

			MissingImage = false;
			if (!HideOverlay)
				Page = Set.Page.ToString();
			if (!HideOverlay && ShowExtraDetails)
			{
				Format = Set.Image!.Substring(Set.Image.LastIndexOf('.') + 1).ToUpper();
				var size = await Images.GetImageSizeCached(Set.Image);
				Resolution = $"{size.Width}x{size.Height}";
			}

			Thumbnail = await ImageProcessing.ByteToBitmap(await Images.GetThumbnailCached(Set.Id, Set.Page), decodeHeight: 275, image: Thumbnail);

			if (Thumbnail != null)
				await Show.InvokeAsync(Platform.AnimationsEnabled);
			else
				MissingImage = true;
			_loading = false;
		}
	}
}
