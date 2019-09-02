using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
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
			get => _isLoading;
			set
			{
				_isLoading = value;
				RaisePropertyChanged("IsLoading");
			}
		}
		private bool _loadingArchives = false;
		public bool LoadingArchives
		{
			get => _loadingArchives;
			set
			{
				_loadingArchives = value;
				RaisePropertyChanged("LoadingArchives");
				RaisePropertyChanged("ControlsEnabled");
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
				RaisePropertyChanged("ControlsEnabled");
			}
		}
		private ObservableCollection<Archive> _archiveList = new ObservableCollection<Archive>();
		public ObservableCollection<Archive> ArchiveList => _archiveList;
		private bool _newOnly;
		public bool NewOnly
		{
			get => _newOnly;
			set
			{
				_newOnly = value;
				RaisePropertyChanged("NewOnly");
			}
		}
		public bool ControlsEnabled
		{
			get => !LoadingArchives && !RefreshOnErrorButton;
		}
		public async Task Refresh()
		{
			await Refresh(true);
		}

		public async Task Refresh(bool animate)
		{
			ArchiveList.Clear();
			if (animate)
				LoadingArchives = true;
			RefreshOnErrorButton = false;

			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/archivelist");

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<List<Archive>>(r);

			if (animate)
				LoadingArchives = false;
			if (!r.IsSuccessful)
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
						foreach (var a in result.Data.OrderBy(a => a.title))
						{
							await DispatcherHelper.RunAsync(() => ArchiveList.Add(a));
						}
					});
					break;
				case HttpStatusCode.Unauthorized:
					RefreshOnErrorButton = true;
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}

		}
	}
}
