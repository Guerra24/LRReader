using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public partial class TankoubonsViewModel : ObservableObject, IRecipient<DeleteTankoubonMessage>
	{
		private readonly SettingsService Settings;
		private readonly IDispatcherService Dispatcher;
		private readonly PlatformService Platform;
		private readonly TabsService Tabs;
		private readonly ApiService Api;

		[ObservableProperty]
		[NotifyPropertyChangedFor("ControlsEnabled")]
		private bool _loadingCategories = true;
		[ObservableProperty]
		[NotifyPropertyChangedFor("ControlsEnabled")]
		private bool _refreshOnErrorButton;
		public ObservableCollection<Tankoubon> Tankoubons = new ObservableCollection<Tankoubon>();
		[ObservableProperty]
		private int _page;
		[ObservableProperty]
		[NotifyPropertyChangedFor("TotalPages")]
		private int _totalTankoubons;

		public int TotalPages => (int)Math.Max(Math.Ceiling(TotalTankoubons / (double)Api.ServerInfo.archives_per_page), 1);
		public bool HasNextPage => Page < TotalPages - 1 && ControlsEnabled;
		public bool HasPrevPage => Page > 0 && ControlsEnabled;

		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set => SetProperty(ref _controlsEnabled, value);
		}
		protected bool _internalLoadingCategories;

		public TankoubonsViewModel(SettingsService settings, IDispatcherService dispatcher, PlatformService platform, TabsService tabs, ApiService api)
		{
			Settings = settings;
			Dispatcher = dispatcher;
			Platform = platform;
			Tabs = tabs;
			Api = api;
			WeakReferenceMessenger.Default.Register(this);
		}

		[RelayCommand]
		private async Task TankoubonClick(GridViewExtParameter item)
		{
			if (item.Item is AddNewTankoubon)
			{
				var dialog = Platform.CreateDialog<ICreateTankoubonDialog>(Dialog.CreateTankoubon);
				var result = await Platform.ShowDialog(dialog);
				if (result == IDialogResult.Primary)
				{
					var resultCreate = await TankoubonsProvider.CreateTankoubon(dialog.Name);
					if (resultCreate != null)
					{
						Tankoubons.Add(resultCreate);
						//Tabs.OpenTab(Tab.CategoryEdit, resultCreate);
					}
				}
			}
			else
			{
				Tabs.OpenTab(Tab.Tankoubon, item.Ctrl, item.Item);
			}
		}

		[RelayCommand]
		public async Task Refresh() => await LoadPage(0);

		public async Task LoadPage(int page)
		{
			if (_internalLoadingCategories)
				return;
			ControlsEnabled = false;
			_internalLoadingCategories = true;
			RefreshOnErrorButton = false;
			LoadingCategories = true;
			Tankoubons.Clear();
			var result = await TankoubonsProvider.GetTankoubons();
			if (result != null)
			{
				if (Settings.Profile.HasApiKey)
					Tankoubons.Add(new AddNewTankoubon());
				TotalTankoubons = result.total;
				await Task.Run(async () =>
				{
					foreach (var a in result.result)
						await Dispatcher.RunAsync(() => Tankoubons.Add(a));
				});
			}
			else
				RefreshOnErrorButton = true;
			LoadingCategories = false;
			_internalLoadingCategories = false;
			ControlsEnabled = true;
		}

		// Edit and view tabs, both with paging
		// Add archives to tanks inside each archive (just like categories)
		// Add archives to tanks from context menu
		// When deleting archive also remove it manually from each tank and category

		public void Receive(DeleteTankoubonMessage message)
		{
			Tankoubons.Remove(message.Value);
		}
	}
}
