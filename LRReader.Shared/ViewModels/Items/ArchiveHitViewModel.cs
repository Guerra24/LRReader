using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Items
{

	public partial class ArchiveHitViewModel : ObservableObject
	{
		[AllowNull]
		private ArchiveHit _archiveHit;
		public ArchiveHit ArchiveHit
		{
			get => _archiveHit;
			set => SetProperty(ref _archiveHit, value);
		}
	}

	public partial class ArchiveHitPreviewViewModel : ArchiveBaseViewModel
	{
		private readonly IDispatcherService Dispatcher;
		private readonly ImagesService Images;

		[ObservableProperty]
		private bool _loadingImages = false;
		public ObservableCollection<ImagePageSet> ArchiveImages = new ObservableCollection<ImagePageSet>();
		private bool _internalLoadingImages;

		private bool _loading;

		[ObservableProperty]
		private string _resolution = "???x???";

		public ArchiveHitPreviewViewModel(
			SettingsService settings,
			ArchivesService archives,
			IDispatcherService dispatcher,
			ApiService api,
			PlatformService platform,
			TabsService tabs,
			ImagesService images) : base(settings, archives, api, platform, tabs)
		{
			Dispatcher = dispatcher;
			Images = images;
		}


		[RelayCommand]
		public async Task Reload(bool animate = true)
		{
			if (_loading)
				return;
			_loading = true;
			ControlsEnabled = false;
			ArchiveImages.Clear();
			Pages = 0;
			Resolution = "???x???";
			await LoadArchive();
			await LoadImages(animate);
			ControlsEnabled = true;
			_loading = false;
		}

		public async Task LoadImages(bool animate = true)
		{
			if (_internalLoadingImages)
				return;
			_internalLoadingImages = true;
			RefreshOnErrorButton = false;
			var result = await ArchivesProvider.ExtractArchive(Archive.arcid);
			if (result != null)
			{
				if (result.job > 0)
				{
					LoadingImages = animate && true;
					await result.WaitForMinionJob();
					LoadingImages = false;
				}
				Pages = result.pages.Count;
				var sizeTask = Task.Run(async () =>
				{
					Size size = Size.Empty;
					foreach (var (s, index) in result.pages.Select((item, index) => (item, index)))
					{
						var image = await Images.GetImageSizeCached(s);
						size += image;
					}
					await Dispatcher.RunAsync(() => Resolution = $"{size.Width / result.pages.Count}x{size.Height / result.pages.Count}", 10);
				});
				var imagesTask = Task.Run(async () =>
				{
					foreach (var (s, index) in result.pages.Select((item, index) => (item, index)))
						await Dispatcher.RunAsync(() => ArchiveImages.Add(new ImagePageSet(Archive.arcid, s, index + 1)), 10);
				});
				await sizeTask;
				await imagesTask;
			}
			else
				RefreshOnErrorButton = true;
			_internalLoadingImages = false;
		}

	}
}
