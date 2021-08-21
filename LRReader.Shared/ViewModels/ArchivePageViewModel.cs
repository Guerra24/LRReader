using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using System;
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
		private readonly ImageProcessingService ImageProcessing;
		private readonly ImagesService Images;
		private readonly EventsService Events;

		private bool _loadingImages = false;
		public bool LoadingImages
		{
			get => _loadingImages;
			set => SetProperty(ref _loadingImages, value);
		}
		private bool _loadingIndeterminate = false;
		public bool LoadingIndeterminate
		{
			get => _loadingImages;
			set => SetProperty(ref _loadingIndeterminate, value);
		}
		public ObservableCollection<ImagePageSet> ArchiveImages = new ObservableCollection<ImagePageSet>();
		public ObservableCollection<ReaderImageSet> ArchiveImagesReader = new ObservableCollection<ReaderImageSet>();
		private bool _showReader = false;
		public bool ShowReader
		{
			get => _showReader;
			set
			{
				SetProperty(ref _showReader, value);
				ControlsEnabled = !value;
			}
		}
		public override bool Downloading
		{
			get => _downloading || !ControlsEnabled;
			set => SetProperty(ref _downloading, value);
		}
		private bool _internalLoadingImages;
		private ReaderImageSet _readerContent;
		public ReaderImageSet ReaderContent
		{
			get => _readerContent;
			set => SetProperty(ref _readerContent, value);
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

		private int _buildProgress;
		public int BuildProgress
		{
			get => _buildProgress;
			set => SetProperty(ref _buildProgress, value);
		}
		private int _buildMax;
		public int BuildMax
		{
			get => _buildMax;
			set => SetProperty(ref _buildMax, value);
		}

		private bool _loading, _abort;

		public ArchivePageViewModel(
			SettingsService settings,
			ArchivesService archives,
			IDispatcherService dispatcher,
			ApiService api,
			IPlatformService platform,
			ImageProcessingService imageProcessing,
			ImagesService images,
			EventsService events) : base(settings, archives, api, platform)
		{
			Dispatcher = dispatcher;
			ImageProcessing = imageProcessing;
			Images = images;
			Events = events;
			_zoomValue = Settings.DefaultZoom;

			Events.RebuildReaderImagesSetEvent += CreateImageSets;
		}

		public void UnHook()
		{
			Events.RebuildReaderImagesSetEvent -= CreateImageSets;
			_abort = true;
		}

		public async Task Reload(bool animate)
		{
			if (_loading)
				return;
			_loading = true;
			ControlsEnabled = false;
			await LoadArchive();
			await LoadImages(animate);
			await CreateImageSetsAsync();
			OnPropertyChanged("Icon");
			ControlsEnabled = true;
			_loading = false;
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
				LoadingIndeterminate = LoadingImages = true;
			ArchiveImages.Clear();
			var result = await ArchivesProvider.ExtractArchive(Archive.arcid);
			if (animate)
				LoadingIndeterminate = LoadingImages = false;
			if (result != null)
			{
				await Task.Run(async () =>
				{
					foreach (var (s, index) in result.pages.Select((item, index) => (item, index)))
						await Dispatcher.RunAsync(() => ArchiveImages.Add(new ImagePageSet(s, index + 1)), -10);
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
			var tmp = new List<ReaderImageSet>();
			BuildMax = ArchiveImages.Count - 1;
			LoadingImages = Settings.TwoPages && Settings.ReaderImageSetBuilder;
			await Task.Run(async () =>
			{
				for (int k = 0; k < ArchiveImages.Count; k++)
				{
					if (_abort)
						return;
					var i = new ReaderImageSet();
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

								if (Settings.ReaderImageSetBuilder)
								{
									var leftImage = ArchiveImages.ElementAt(k + 1).Image;
									var leftSize = await Images.GetImageSizeCached(i.RightImage);
									var rightSize = await Images.GetImageSizeCached(leftImage);
									if (Math.Abs(leftSize.Width / (double)leftSize.Height - rightSize.Width / (double)rightSize.Height) <= 0.1)
									{
										i.LeftImage = leftImage;
										k++;
										i.TwoPages = true;
									}
									else
									{
										i.LeftImage = i.RightImage;
										i.RightImage = null;
									}
								}
								else
								{
									i.LeftImage = ArchiveImages.ElementAt(++k).Image;
									i.TwoPages = true;
								}
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

								if (Settings.ReaderImageSetBuilder)
								{
									var rightImage = ArchiveImages.ElementAt(k + 1).Image;
									var leftSize = await Images.GetImageSizeCached(i.LeftImage);
									var rightSize = await Images.GetImageSizeCached(rightImage);
									if (Math.Abs(leftSize.Width / (double)leftSize.Height - rightSize.Width / (double)rightSize.Height) <= 0.1)
									{
										i.RightImage = rightImage;
										k++;
										i.TwoPages = true;
									}
								}
								else
								{
									i.RightImage = ArchiveImages.ElementAt(++k).Image;
									i.TwoPages = true;
								}
							}
						}
					}
					else
					{
						i.LeftImage = ArchiveImages.ElementAt(k).Image;
					}
					i.Page = k;
					Dispatcher.Run(() => BuildProgress = k);
					tmp.Add(i);
				}
			});
			LoadingImages = false;
			foreach (var i in tmp)
			{
				ArchiveImagesReader.Add(i);
			}
		}
	}
}
