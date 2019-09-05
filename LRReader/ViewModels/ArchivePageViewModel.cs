using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.Models.Api;
using LRReader.Models.Main;
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
		private ObservableCollection<String> _archiveImages = new ObservableCollection<String>();
		public ObservableCollection<string> ArchiveImages
		{
			get => _archiveImages;
		}
		private ObservableCollection<string> _tags = new ObservableCollection<string>();
		public ObservableCollection<string> Tags
		{
			get => _tags;
		}
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
			LoadingImages = true;
			RefreshOnErrorButton = false;
			ArchiveImages.Clear();

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/extract");

			rq.AddParameter("id", Archive.arcid);

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<ArchiveImages>(r);

			LoadingImages = false;
			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				RefreshOnErrorButton = true;
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
	}
}
