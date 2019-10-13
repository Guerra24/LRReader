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
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				_refreshOnErrorButton = value;
				RaisePropertyChanged("RefreshOnErrorButton");
			}
		}
		public ObservableCollection<string> ArchiveImages = new ObservableCollection<string>();
		public ObservableCollection<ArchiveImageSet> ArchiveImagesReader = new ObservableCollection<ArchiveImageSet>();
		public ObservableCollection<string> Tags = new ObservableCollection<string>();
		private bool _showReader = false;
		public bool ShowReader
		{
			get => _showReader;
			set
			{
				_showReader = value;
				RaisePropertyChanged("ShowReader");
			}
		}
		private bool _internalLoadingImages;

		public async void Reload(bool animate)
		{
			LoadTags();
			await LoadImages(animate);
			CreateImageSets();
			RaisePropertyChanged("Bookmarked");
			RaisePropertyChanged("Icon");
		}

		public void LoadTags()
		{
			Tags.Clear();

			foreach (var s in Archive.tags.Split(","))
			{
				Tags.Add(s.Trim());
			}
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
			if (animate)
				LoadingImages = true;
			RefreshOnErrorButton = false;
			ArchiveImages.Clear();

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/extract");

			rq.AddParameter("id", Archive.arcid);

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<ArchiveImages>(r);

			if (animate)
				LoadingImages = false;
			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				RefreshOnErrorButton = true;
				_internalLoadingImages = false;
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					await Task.Run(async () =>
					{
						foreach (var s in result.Data.pages)
						{
							await DispatcherHelper.RunAsync(() => ArchiveImages.Add(s));
						}
					});
					break;
				case HttpStatusCode.Unauthorized:
					RefreshOnErrorButton = true;
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}
			_internalLoadingImages = false;
		}

		public async void ClearNew()
		{
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/clear_new");

			rq.AddParameter("id", Archive.arcid);

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<ApiResult>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					break;
				case HttpStatusCode.Unauthorized:
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}
		}

		public async void CreateImageSets()
		{
			ArchiveImagesReader.Clear();
			List<ArchiveImageSet> tmp = new List<ArchiveImageSet>();
			await Task.Run(() =>
			{
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
			});
			foreach (var i in tmp)
			{
				ArchiveImagesReader.Add(i);
			}
		}
	}
}
