using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
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

namespace LRReader.ViewModels
{
	public class ArchivePageViewModel : ViewModelBase
	{
		private bool _isLoading = false;
		public bool IsLoading
		{
			get
			{
				return _isLoading;
			}
			set
			{
				_isLoading = value;
				RaisePropertyChanged("IsLoading");
			}
		}
		private bool _loadingImages = false;
		public bool LoadingImages
		{
			get
			{
				return _loadingImages;
			}
			set
			{
				_loadingImages = value;
				RaisePropertyChanged("LoadingImages");
			}
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get
			{
				return _refreshOnErrorButton;
			}
			set
			{
				_refreshOnErrorButton = value;
				RaisePropertyChanged("RefreshOnErrorButton");
			}
		}
		private ObservableCollection<String> _archiveImages = new ObservableCollection<String>();
		public ObservableCollection<string> ArchiveImages
		{
			get
			{
				return _archiveImages;
			}
		}
		private Archive _archive = new Archive();
		public Archive Archive
		{
			get
			{
				return _archive;
			}
			set
			{
				if (!_archive.Equals(value))
				{
					_archive = value;
					RaisePropertyChanged("Archive");
				}
			}
		}

		public void LoadImages()
		{
			LoadingImages = true;
			RefreshOnErrorButton = false;

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/extract");

			rq.AddParameter("id", Archive.arcid);

			ArchiveImages.Clear();
			/*var r = client.Get<ArchiveImages>(rq);

			foreach (var s in r.Data.pages)
			{
				ArchiveImages.Add(client.BaseUrl + s);
			}*/
			client.GetAsync<ArchiveImages>(rq, async (r, h) =>
		   {
			   await DispatcherHelper.RunAsync(() => LoadingImages = false);
			   if (!r.IsSuccessful)
			   {
				   await DispatcherHelper.RunAsync(() => RefreshOnErrorButton = true);
				   Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				   return;
			   }
			   foreach (var s in r.Data.pages)
			   {
				   await DispatcherHelper.RunAsync(() => ArchiveImages.Add(client.BaseUrl + s));
			   }
		   });
		}
	}
}
