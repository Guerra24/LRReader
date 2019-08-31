using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.Models.Api;
using LRReader.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.ViewModels.Items
{
	public class ArchiveItemViewModel : ViewModelBase
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
		private Archive _archive = new Archive() { arcid = "", isnew = "" };
		public Archive Archive
		{
			get => _archive;
			set
			{
				if (_archive != value)
				{
					if (!_archive.arcid.Equals(value.arcid))
					{
						_archive = value;
						RaisePropertyChanged("Archive");
						RaisePropertyChanged("IsNew");
					}
				}
			}
		}
		public bool IsNew
		{
			get => _archive != null ? _archive.isnew.Equals("block") : false;
		}

		public async Task<DownloadPayload> DownloadArchive()
		{
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/servefile");

			rq.AddParameter("id", Archive.arcid);

			var r = await client.ExecuteGetTaskAsync(rq);

			if (!r.IsSuccessful)
			{
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			if (r.StatusCode == HttpStatusCode.OK)
			{
				var download = new DownloadPayload();
				var header = r.Headers.First(h => h.Name.Equals("Content-Disposition")).Value as string;
				var parms = header.Split(";").Select(s => s.Trim());
				var natr = parms.First(s => s.StartsWith("filename"));
				var nameAndType = natr.Substring(natr.IndexOf("\"") + 1, natr.Length - natr.IndexOf("\"") - 2);

				download.Data = r.RawBytes;
				download.Name = nameAndType.Substring(0, nameAndType.LastIndexOf("."));
				download.Type = nameAndType.Substring(nameAndType.LastIndexOf("."));
				return download;
			}
			return null;
		}
	}
	public class DownloadPayload
	{
		public byte[] Data { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
	}
}
