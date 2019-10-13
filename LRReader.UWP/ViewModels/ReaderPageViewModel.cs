using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Models.Api;
using LRReader.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace LRReader.ViewModels
{
	public class ReaderPageViewModel : ViewModelBase
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
		private ObservableCollection<string> _archiveImages = new ObservableCollection<string>();
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
			ArchiveImages.Clear();

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/extract");

			rq.AddParameter("id", Archive.arcid);

			var r = client.Get(rq);

			var result = LRRApi.GetResult<ArchiveImages>(r);

			if (!r.IsSuccessful)
			{
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					foreach (var s in result.Data.pages)
					{
						ArchiveImages.Add(s);
					}
					break;
				case HttpStatusCode.Unauthorized:
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}
		}
	}
}
