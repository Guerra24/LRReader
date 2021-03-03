using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.UWP.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{

	public delegate void ZoomChanged();

	public class ArchivePageViewModel : ArchiveBaseViewModel
	{
		private bool _loadingImages = false;
		public bool LoadingImages
		{
			get => _loadingImages;
			set
			{
				_loadingImages = value;
				RaisePropertyChanged("LoadingImages");
			}
		}
		public ObservableCollection<ImagePageSet> ArchiveImages = new ObservableCollection<ImagePageSet>();
		public ObservableCollection<ArchiveImageSet> ArchiveImagesReader = new ObservableCollection<ArchiveImageSet>();
		private bool _showReader = false;
		public bool ShowReader
		{
			get => _showReader;
			set
			{
				if (_showReader != value)
				{
					_showReader = value;
					RaisePropertyChanged("ShowReader");
				}
			}
		}
		public override bool Downloading
		{
			get => _downloading || !ControlsEnabled;
			set
			{
				_downloading = value;
				RaisePropertyChanged("Downloading");
			}
		}
		private bool _internalLoadingImages;
		private ArchiveImageSet _readerContent;
		public ArchiveImageSet ReaderContent
		{
			get => _readerContent;
			set
			{
				_readerContent = value;
				RaisePropertyChanged("ReaderContent");
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
				RaisePropertyChanged("ReaderProgress");
			}
		}
		public int ReaderProgress => ReaderIndex + 1;
		private int _zoomValue = Global.SettingsManager.DefaultZoom;
		public int ZoomValue
		{
			get => _zoomValue;
			set
			{
				if (value != _zoomValue)
				{
					_zoomValue = value;
					RaisePropertyChanged("ZoomValue");
					ZoomChangedEvent?.Invoke();
				}
			}
		}
		public event ZoomChanged ZoomChangedEvent;

		public async Task Reload(bool animate)
		{
			ControlsEnabled = false;
			await LoadArchive();
			await LoadImages(animate);
			CreateImageSets();
			RaisePropertyChanged("Icon");
			ControlsEnabled = true;
		}

		public void ReloadBookmarkedObject()
		{
			BookmarkedArchive = Global.SettingsManager.Profile.Bookmarks.FirstOrDefault(b => b.archiveID.Equals(Archive.arcid));
			RaisePropertyChanged("Icon");
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
						await DispatcherHelper.RunAsync(() => ArchiveImages.Add(new ImagePageSet(s, index + 1)));
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

		public void CreateImageSets()
		{
			ArchiveImagesReader.Clear();
			List<ArchiveImageSet> tmp = new List<ArchiveImageSet>();
			for (int k = 0; k < ArchiveImages.Count; k++)
			{
				var i = new ArchiveImageSet();
				if (Global.SettingsManager.TwoPages)
				{
					if (Global.SettingsManager.ReadRTL)
					{
						if (k == 0)
							i.RightImage = ArchiveImages.ElementAt(k).Image;
						else if (k == ArchiveImages.Count - 1)
							i.LeftImage = ArchiveImages.ElementAt(k).Image;
						else
						{
							i.RightImage = ArchiveImages.ElementAt(k).Image;
							i.LeftImage = ArchiveImages.ElementAt(++k).Image;
						}
					}
					else
					{
						if (k == 0)
							i.RightImage = ArchiveImages.ElementAt(k).Image;
						else if (k == ArchiveImages.Count - 1)
							i.LeftImage = ArchiveImages.ElementAt(k).Image;
						else
						{
							i.LeftImage = ArchiveImages.ElementAt(k).Image;
							i.RightImage = ArchiveImages.ElementAt(++k).Image;
						}
					}
				}
				else
				{
					i.LeftImage = ArchiveImages.ElementAt(k).Image;
				}
				tmp.Add(i);
			}
			if (Global.SettingsManager.ReadRTL)
				tmp.Reverse();
			foreach (var i in tmp)
			{
				ArchiveImagesReader.Add(i);
			}
		}
	}
}
