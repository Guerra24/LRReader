using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Net;
using LRReader.ViewModels.Base;
using static LRReader.Shared.Providers.Providers;

namespace LRReader.ViewModels
{
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
		public ObservableCollection<string> ArchiveImages = new ObservableCollection<string>();
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
		private Object _readerContent;
		public Object ReaderContent
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
			}
		}

		public async Task Reload(bool animate)
		{
			ControlsEnabled = false;
			LoadTags();
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

		public async Task LoadImages()
		{
			await LoadImages(true);
		}

		public async Task LoadImages(bool animate)
		{
			if (_internalLoadingImages)
				return;
			_internalLoadingImages = true;
			RefreshOnErrorButton = false;
			if (animate)
				LoadingImages = true;
			ArchiveImages.Clear();
			var result = await ImagesProvider.LoadImages(Archive);
			if (animate)
				LoadingImages = false;
			if (result != null)
			{
				await Task.Run(async () =>
				{
					foreach (var s in result)
						await DispatcherHelper.RunAsync(() => ArchiveImages.Add(s));
				});
				Pages = ArchiveImages.Count;
			}
			else
				RefreshOnErrorButton = true;
			_internalLoadingImages = false;
		}

		public async Task ClearNew()
		{
			await Archive.ClearNew();
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
							i.RightImage = ArchiveImages.ElementAt(k);
						else if (k == ArchiveImages.Count - 1)
							i.LeftImage = ArchiveImages.ElementAt(k);
						else
						{
							i.RightImage = ArchiveImages.ElementAt(k);
							i.LeftImage = ArchiveImages.ElementAt(++k);
						}
					}
					else
					{
						if (k == 0)
							i.LeftImage = ArchiveImages.ElementAt(k);
						else if (k == ArchiveImages.Count - 1)
							i.RightImage = ArchiveImages.ElementAt(k);
						else
						{
							i.LeftImage = ArchiveImages.ElementAt(k);
							i.RightImage = ArchiveImages.ElementAt(++k);
						}
					}
				}
				else
				{
					i.LeftImage = ArchiveImages.ElementAt(k);
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
