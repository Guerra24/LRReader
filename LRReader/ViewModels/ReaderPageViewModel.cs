using GalaSoft.MvvmLight;
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
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/extract");

			rq.AddParameter("id", Archive.arcid);

			ArchiveImages.Clear();
			var r = client.Get<ArchiveImages>(rq);

			foreach (var s in r.Data.pages)
			{
				ArchiveImages.Add(s);
			}
		}
	}
}
