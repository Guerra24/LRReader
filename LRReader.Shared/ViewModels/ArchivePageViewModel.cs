using LRReader.Shared.Extensions;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{

	public delegate void ZoomChanged();

	public partial class ArchivePageViewModel : ArchiveBaseViewModel
	{
		private readonly IDispatcherService Dispatcher;
		private readonly ImagesService Images;
		private readonly EventsService Events;

		[ObservableProperty]
		[AlsoNotifyChangeFor("CanGoNext")]
		private IList<Archive> _group = new List<Archive>();

		public bool CanGoNext => Group.Count != 0 && Group.ElementAtOrDefault(Group.IndexOf(Archive) + 1) != null;

		[ObservableProperty]
		private bool _loadingImages = false;
		[ObservableProperty]
		private bool _loadingIndeterminate = false;
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
		[ObservableProperty]
		[AllowNull]
		private ReaderImageSet _readerContent;
		private int _readerIndex;
		public int ReaderIndex
		{
			get => _readerIndex;
			set
			{
				if (ArchiveImagesReader.Count <= value)
					return;
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
				if (TwoPages)
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
					SetProperty(ref _zoomValue, value);
					ZoomChangedEvent?.Invoke();
				}
			}
		}
		public event ZoomChanged? ZoomChangedEvent;

		private bool _readRTL;
		public bool ReadRTL
		{
			get => _readRTL;
			set
			{
				if (value != _readRTL)
				{
					SetProperty(ref _readRTL, value);
					RebuildReader?.Invoke();
				}
			}
		}
		private bool _twoPages;
		public bool TwoPages
		{
			get => _twoPages;
			set
			{
				if (value != _twoPages)
				{
					SetProperty(ref _twoPages, value);
					RebuildReader?.Invoke();
				}
			}
		}
		private bool _setBuilder;
		public bool SetBuilder
		{
			get => _setBuilder;
			set
			{
				if (value != _setBuilder)
				{
					SetProperty(ref _setBuilder, value);
					RebuildReader?.Invoke();
				}
			}
		}
		private bool _fitToWidth;
		public bool FitToWidth
		{
			get => _fitToWidth;
			set
			{
				if (value != _fitToWidth)
				{
					SetProperty(ref _fitToWidth, value);
					ZoomChangedEvent?.Invoke();
				}
			}
		}
		private int _fitScaleLimit;
		public int FitScaleLimit
		{
			get => _fitScaleLimit;
			set
			{
				if (value != _fitScaleLimit)
				{
					SetProperty(ref _fitScaleLimit, value);
					ZoomChangedEvent?.Invoke();
				}
			}
		}
		private bool _useVerticalReader;
		public bool UseVerticalReader
		{
			get => _useVerticalReader;
			set
			{
				if (value != _useVerticalReader)
				{
					SetProperty(ref _useVerticalReader, value);
					RebuildReader?.Invoke();
				}
			}
		}

		public event Action? RebuildReader;

		[ObservableProperty]
		private int _buildProgress;
		[ObservableProperty]
		private int _buildMax;

		private bool _loading, _abort;

		public ArchivePageViewModel(
			SettingsService settings,
			ArchivesService archives,
			IDispatcherService dispatcher,
			ApiService api,
			PlatformService platform,
			TabsService tabs,
			ImagesService images,
			EventsService events) : base(settings, archives, api, platform, tabs)
		{
			Dispatcher = dispatcher;
			Images = images;
			Events = events;
			_zoomValue = Settings.DefaultZoom;
			_readRTL = Settings.ReadRTL;
			_twoPages = Settings.TwoPages;
			_setBuilder = Settings.ReaderImageSetBuilder;
			_fitToWidth = Settings.FitToWidth;
			_fitScaleLimit = Settings.FitScaleLimit;
			_useVerticalReader = Settings.UseVerticalReader;
		}

		public void UnHook()
		{
			_abort = true;
		}

		public async Task HandleConflict()
		{
			if (Api.ControlFlags.ProgressTracking && Bookmarked && BookmarkProgress + 1 != Archive.progress && Archive.progress > 0)
			{
				var dialog = Platform.CreateDialog<IProgressConflictDialog>(Dialog.ProgressConflict, BookmarkProgress + 1, Archive.progress, Pages);
				await dialog.ShowAsync();
				switch (dialog.Mode)
				{
					case ConflictMode.Local:
						await SetProgress(BookmarkProgress + 1);
						break;
					case ConflictMode.Remote:
						BookmarkProgress = Archive.progress - 1;
						break;
				}
			}
		}

		public async Task<bool> SaveReaderData(bool _wasNew)
		{
			int currentPage = ReaderContent.Page;
			int count = Pages;

			if (currentPage >= count - Math.Min(10, Math.Ceiling(count * 0.1)))
			{
				if (Archive.isnew)
				{
					await ClearNew();
					Archive.isnew = false;
				}
				if (Bookmarked && Settings.RemoveBookmark)
				{
					var result = await Platform.OpenGenericDialog(
						Platform.GetLocalizedString("Tabs/Archive/RemoveBookmark/Title"),
						Platform.GetLocalizedString("Tabs/Archive/RemoveBookmark/PrimaryButtonText"),
						closebutton: Platform.GetLocalizedString("Tabs/Archive/RemoveBookmark/CloseButtonText")
						);
					if (result == IDialogResult.Primary)
						Bookmarked = false;
				}
			}
			else if (!Bookmarked)
			{
				var mode = Service.Settings.BookmarkReminderMode;
				if (Service.Settings.BookmarkReminder &&
					((_wasNew && mode == BookmarkReminderMode.New) || mode == BookmarkReminderMode.All))
				{
					var result = await Platform.OpenGenericDialog(
						Platform.GetLocalizedString("Tabs/Archive/AddBookmark/Title"),
						Platform.GetLocalizedString("Tabs/Archive/AddBookmark/PrimaryButtonText"),
						closebutton: Platform.GetLocalizedString("Tabs/Archive/AddBookmark/CloseButtonText")
						);
					if (result == IDialogResult.Primary)
						Bookmarked = true;
					_wasNew = false;
				}
			}
			if (Bookmarked)
			{
				BookmarkProgress = currentPage;
				if (Api.ControlFlags.ProgressTracking)
					await SetProgress(currentPage + 1);
			}
			return _wasNew;
		}

		[ICommand]
		public async Task Reload(bool animate = true)
		{
			if (_loading)
				return;
			_loading = true;
			ControlsEnabled = false;
			await LoadArchive();
			await LoadImages(animate);
			await CreateImageSets();
			OnPropertyChanged("Icon");
			ControlsEnabled = true;
			_loading = false;
		}

		public async Task NextArchive()
		{
			if (Group.Count == 0)
				return;
			int i = Group.IndexOf(Archive);
			var next = Group.ElementAtOrDefault(i + 1);
			if (next == null)
				return;
			Archive = next;
			OnPropertyChanged("CanGoNext");
			await Reload(true);
		}

		public async Task OpenArchive(Archive archive)
		{
			Group = new List<Archive>();
			Archive = archive;
			OnPropertyChanged("CanGoNext");
			await Reload(true);
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
			if (result != null)
				await result.WaitForMinionJob();
			if (animate)
				LoadingIndeterminate = LoadingImages = false;
			if (result != null)
			{
				await Task.Run(async () =>
				{
					foreach (var (s, index) in result.pages.Select((item, index) => (item, index)))
						await Dispatcher.RunAsync(() => ArchiveImages.Add(new ImagePageSet(Archive.arcid, s, index + 1)), -10);
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

		public async Task CreateImageSets()
		{
			ArchiveImagesReader.Clear();
			var tmp = new List<ReaderImageSet>();
			BuildMax = ArchiveImages.Count - 1;
			LoadingImages = (TwoPages && SetBuilder) || UseVerticalReader;
			double MaxWidth = 0;
			await Task.Run(async () =>
			{
				for (int k = 0; k < ArchiveImages.Count; k++)
				{
					if (_abort)
						return;
					var i = new ReaderImageSet();
					if (TwoPages && !UseVerticalReader)
					{
						if (ReadRTL)
						{
							if (k == 0)
								i.LeftImage = ArchiveImages.ElementAt(k).Image;
							else if (k == ArchiveImages.Count - 1)
								i.LeftImage = ArchiveImages.ElementAt(k).Image;
							else
							{
								i.RightImage = ArchiveImages.ElementAt(k).Image;

								if (SetBuilder)
								{
									var leftImage = ArchiveImages.ElementAt(k + 1).Image;
									var leftSize = await Images.GetImageSizeCached(i.RightImage!);
									var rightSize = await Images.GetImageSizeCached(leftImage!);
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

								if (SetBuilder)
								{
									var rightImage = ArchiveImages.ElementAt(k + 1).Image;
									var leftSize = await Images.GetImageSizeCached(i.LeftImage);
									var rightSize = await Images.GetImageSizeCached(rightImage!);
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
					else if (UseVerticalReader)
					{
						var image = ArchiveImages.ElementAt(k).Image;
						var size = await Images.GetImageSizeCached(image!);
						i.Width = size.Width;
						i.Height = size.Height;
						i.LeftImage = image;
						MaxWidth = Math.Max(MaxWidth, i.Width);
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
				if (UseVerticalReader)
				{
					var aspect = i.Height / i.Width;
					i.Height = MaxWidth * aspect;
				}
				ArchiveImagesReader.Add(i);
			}
		}
	}
}
