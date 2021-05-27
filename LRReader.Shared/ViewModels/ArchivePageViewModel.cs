using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{

	public delegate void ZoomChanged();

	public class ArchivePageViewModel : ArchiveBaseViewModel
	{
		private readonly IDispatcherService Dispatcher;
		private readonly IImageProcessingService ImageProcessing;
		private readonly ImagesService Images;

		private bool _loadingImages = false;
		public bool LoadingImages
		{
			get => _loadingImages;
			set => SetProperty(ref _loadingImages, value);
		}
		public ObservableCollection<ImagePageSet> ArchiveImages = new ObservableCollection<ImagePageSet>();
		public ObservableCollection<ArchiveImageSet> ArchiveImagesReader = new ObservableCollection<ArchiveImageSet>();
		private bool _showReader = false;
		public bool ShowReader
		{
			get => _showReader;
			set => SetProperty(ref _showReader, value);
		}
		public override bool Downloading
		{
			get => _downloading || !ControlsEnabled;
			set => SetProperty(ref _downloading, value);
		}
		private bool _internalLoadingImages;
		private ArchiveImageSet _readerContent;
		public ArchiveImageSet ReaderContent
		{
			get => _readerContent;
			set
			{
				_readerContent = value;
				OnPropertyChanged("ReaderContent");
			}
		}
		private int _readerIndex;
		public int ReaderIndex
		{
			get => _readerIndex;
			set
			{
				ReaderContent = ArchiveImagesReader.ElementAt(value);
				_readerIndex = value;
				OnPropertyChanged("ReaderProgress");
			}
		}
		public int ReaderProgress
		{
			get
			{
				var progress = ReaderIndex;
				if (Service.Settings.TwoPages)
					if (progress != 0)
						progress *= 2;
				progress = progress.Clamp(0, Pages - 1);
				return progress + 1;
			}
		}

		private int _zoomValue = 0;
		public int ZoomValue
		{
			get => _zoomValue;
			set
			{
				if (value != _zoomValue)
				{
					_zoomValue = value;
					OnPropertyChanged("ZoomValue");
					ZoomChangedEvent?.Invoke();
				}
			}
		}
		public event ZoomChanged ZoomChangedEvent;

		public ArchivePageViewModel(
			SettingsService settings,
			ArchivesService archives,
			IDispatcherService dispatcher,
			ApiService api,
			IPlatformService platform,
			IImageProcessingService imageProcessing,
			ImagesService images) : base(settings, archives, api, platform)
		{
			Dispatcher = dispatcher;
			ImageProcessing = imageProcessing;
			Images = images;
			_zoomValue = Settings.DefaultZoom;
		}

		public async Task Reload(bool animate)
		{
			ControlsEnabled = false;
			await LoadArchive();
			await LoadImages(animate);
			await CreateImageSetsAsync();
			OnPropertyChanged("Icon");
			ControlsEnabled = true;
		}

		public void ReloadBookmarkedObject()
		{
			BookmarkedArchive = Settings.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(Archive.arcid));
			OnPropertyChanged("Icon");
		}

		public async Task LoadImages(bool animate = true)
		{
			if (_internalLoadingImages)
				return;
			_internalLoadingImages = true;
			RefreshOnErrorButton = false;
			if (animate)
				LoadingImages = true;
			ArchiveImages.Clear();
			var result = await ArchivesProvider.ExtractArchive(Archive.arcid);
			if (animate)
				LoadingImages = false;
			if (result != null)
			{
				await Task.Run(async () =>
				{
					foreach (var (s, index) in result.pages.Select((item, index) => (item, index)))
						await Dispatcher.RunAsync(() => ArchiveImages.Add(new ImagePageSet(s, index + 1)));
				});
				Pages = ArchiveImages.Count;
			}
			else
				RefreshOnErrorButton = true;
			_internalLoadingImages = false;
		}

		public async Task ClearNew()
		{
			await ArchivesProvider.ClearNewArchive(Archive.arcid);
		}

		public async void CreateImageSets() => await CreateImageSetsAsync();

		public async Task CreateImageSetsAsync()
		{
			ArchiveImagesReader.Clear();
			var tmp = new List<ArchiveImageSet>();
			await Task.Run(() =>
			{
				for (int k = 0; k < ArchiveImages.Count; k++)
				{
					var i = new ArchiveImageSet();
					if (Settings.TwoPages)
					{
						if (Settings.ReadRTL)
						{
							if (k == 0)
								i.LeftImage = ArchiveImages.ElementAt(k).Image;
							else if (k == ArchiveImages.Count - 1)
								i.LeftImage = ArchiveImages.ElementAt(k).Image;
							else
							{
								i.RightImage = ArchiveImages.ElementAt(k).Image;
								i.LeftImage = ArchiveImages.ElementAt(++k).Image;
								i.TwoPages = true;
							}
						}
						else
						{
							if (k == 0)
								i.LeftImage = ArchiveImages.ElementAt(k).Image;
							else if (k == ArchiveImages.Count - 1)
								i.LeftImage = ArchiveImages.ElementAt(k).Image;
							else
							{
								i.LeftImage = ArchiveImages.ElementAt(k).Image;
								i.RightImage = ArchiveImages.ElementAt(++k).Image;
								i.TwoPages = true;
							}
						}
					}
					else
					{
						//var size = await ImageProcessing.GetImageSize(await Images.GetImageCached(ArchiveImages.ElementAt(k).Image));
						//System.Diagnostics.Debug.WriteLine(size.Width + " " + size.Height);
						i.LeftImage = ArchiveImages.ElementAt(k).Image;
					}
					i.Page = k;
					System.Diagnostics.Debug.WriteLine(k);
					tmp.Add(i);
				}
			});
			foreach (var i in tmp)
			{
				ArchiveImagesReader.Add(i);
			}
		}
	}
}
