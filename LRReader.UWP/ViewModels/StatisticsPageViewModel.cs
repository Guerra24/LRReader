using GalaSoft.MvvmLight;
using LRReader.Internal;
using LRReader.Shared.Models.Api;
using LRReader.Shared.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.ViewModels
{
	public class StatisticsPageViewModel : ViewModelBase
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
		private bool _loadingStats = false;
		public bool LoadingStats
		{
			get => _loadingStats;
			set
			{
				_loadingStats = value;
				RaisePropertyChanged("LoadingStats");
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
		private ObservableCollection<TagStats> _tagStats = new ObservableCollection<TagStats>();
		public ObservableCollection<TagStats> TagStats => _tagStats;

		public async Task LoadTagStats()
		{
			TagStats.Clear();
			LoadingStats = true;
			RefreshOnErrorButton = false;
			var client = Global.LRRApi.GetClient();

			var rq = new RestRequest("api/tagstats");

			var r = await client.ExecuteGetTaskAsync(rq);

			var result = LRRApi.GetResult<List<TagStats>>(r);

			LoadingStats = false;
			if (!r.IsSuccessful)
			{
				RefreshOnErrorButton = true;
				Global.EventManager.ShowError("Network Error", r.ErrorMessage);
				return;
			}
			switch (r.StatusCode)
			{
				case HttpStatusCode.OK:
					int total = 0;
					foreach (var a in result.Data.OrderByDescending(a => a.weight))
					{
						if (total > 20)
							break;
						TagStats.Add(a);
						total++;
					}
					break;
				case HttpStatusCode.Unauthorized:
					RefreshOnErrorButton = true;
					Global.EventManager.ShowError("API Error", result.Error.error);
					break;
			}
		}
	}
}
