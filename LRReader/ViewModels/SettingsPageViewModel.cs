using GalaSoft.MvvmLight;
using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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
	}
}
