using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Messages;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{

	public partial class TankoubonViewModel : TankoubonBaseViewModel, IRecipient<DeleteArchiveMessage>
	{
		protected readonly ArchivesService Archives;
		private readonly IDispatcherService Dispatcher;
		private readonly ApiService Api;

		[ObservableProperty]
		[NotifyPropertyChangedFor("ControlsEnabled")]
		private bool _loadingArchives = true;
		[ObservableProperty]
		[NotifyPropertyChangedFor("ControlsEnabled")]
		private bool _refreshOnErrorButton;
		public ObservableCollection<Archive> ArchiveList { get; } = new();
		[ObservableProperty]
		private int _page;
		[ObservableProperty]
		[NotifyPropertyChangedFor("TotalPages")]
		private int _totalArchives;

		public int TotalPages => (int)Math.Max(Math.Ceiling(TotalArchives / (double)Api.ServerInfo.archives_per_page), 1);
		public bool HasNextPage => Page < TotalPages - 1 && ControlsEnabled;
		public bool HasPrevPage => Page > 0 && ControlsEnabled;

		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set => SetProperty(ref _controlsEnabled, value);
		}
		protected bool _internalLoadingArchives;

		public TankoubonViewModel(PlatformService platform, TabsService tabs, SettingsService settings, ArchivesService archives, IDispatcherService dispatcher, ApiService api) : base(platform, tabs, settings)
		{
			Dispatcher = dispatcher;
			Archives = archives;
			Api = api;

			WeakReferenceMessenger.Default.Register(this);
		}

		public async Task NextPage()
		{
			if (HasNextPage)
				await LoadPage(Page + 1);
		}

		public async Task PrevPage()
		{
			if (HasPrevPage)
				await LoadPage(Page - 1);
		}

		[RelayCommand]
		public async Task Refresh()
		{
			ControlsEnabled = false;
			await LoadPage(0);
			ControlsEnabled = true;
		}

		public async Task LoadPage(int page)
		{
			if (_internalLoadingArchives)
				return;
			ControlsEnabled = false;
			_internalLoadingArchives = true;
			RefreshOnErrorButton = false;
			LoadingArchives = true;
			ArchiveList.Clear();
			Page = page;
			var result = await TankoubonsProvider.GetTankoubon(Tankoubon.id, page);
			if (result != null)
			{
				TotalArchives = result.total;
				await Task.Run(async () =>
				{
					foreach (var a in result.result.archives)
					{
						var archive = await Archives.GetOrAddArchive(a);
						if (archive != null)
							await Dispatcher.RunAsync(() => ArchiveList.Add(archive), 10);
					}
				});
			}
			else
				RefreshOnErrorButton = true;
			LoadingArchives = false;
			_internalLoadingArchives = false;
			ControlsEnabled = true;
		}

		public void Receive(DeleteArchiveMessage message)
		{
			ArchiveList.Remove(message.Value);
		}
	}
}
