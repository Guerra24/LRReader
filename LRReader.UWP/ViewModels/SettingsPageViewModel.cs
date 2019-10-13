using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Models.Api;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.ViewModels
{
	public class SettingsPageViewModel : ViewModelBase
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
		public SettingsManager SettingsManager
		{
			get => Global.SettingsManager;
		}
		public string Version
		{
			get => Util.GetAppVersion();
		}
		private string _cacheSizeInMB;
		public string CacheSizeInMB
		{
			get => _cacheSizeInMB;
			set
			{
				_cacheSizeInMB = value;
				RaisePropertyChanged("CacheSizeInMB");
			}
		}
		private bool _progressCache;
		public bool ProgressCache
		{
			get => _progressCache;
			set
			{
				_progressCache = value;
				RaisePropertyChanged("ProgressCache");
			}
		}
		public async Task UpdateCacheSize()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			CacheSizeInMB = await Global.ImageManager.GetCacheSizeMB();
			ProgressCache = false;
		}
		public async Task ClearCache()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			await Global.ImageManager.ClearCache();
			ProgressCache = false;
		}
		public async void RestartWorker()
		{
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/restart_shinobu");

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
		public async void StopWorker()
		{
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/stop_shinobu");

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
		public async Task<DownloadPayload> DownloadDB()
		{
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/backup");

			var r = await client.ExecuteGetTaskAsync(rq);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return null;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					var download = new DownloadPayload();

					download.Data = r.RawBytes;
					download.Name = "database_backup.json";
					download.Type = ".json";
					return download;
				case HttpStatusCode.Unauthorized:
					Global.EventManager.ShowError("API Error", LRRApi.GetError(r).error);
					break;
			}
			return null;
		}
		public async void ClearAllNew()
		{
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/clear_new_all");

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
