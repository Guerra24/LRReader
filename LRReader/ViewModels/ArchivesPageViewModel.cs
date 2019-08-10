using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Models.Api;
using LRReader.Models.Main;
using RestSharp;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace LRReader.ViewModels
{
	public class ArchivesPageViewModel : ViewModelBase
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
		private ObservableCollection<Archive> _archiveList = new ObservableCollection<Archive>();
		public ObservableCollection<Archive> ArchiveList
		{
			get
			{
				return _archiveList;
			}
		}
		public ArchivesPageViewModel()
		{
		}

		public async Task Refresh()
		{
			ArchiveList.Clear();

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/archivelist");

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<List<Archive>>(r);
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					foreach (var a in result.Data.OrderBy(a => a.title))
					{
						ArchiveList.Add(a);
					}
					break;
				case HttpStatusCode.Unauthorized:
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}

		}
	}
}
