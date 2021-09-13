using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels
{
	public partial class CategoriesViewModel : ObservableObject, IRecipient<DeleteCategoryMessage>
	{
		private readonly SettingsService Settings;
		private readonly IDispatcherService Dispatcher;
		private readonly PlatformService Platform;
		private readonly TabsService Tabs;

		[ObservableProperty]
		[AlsoNotifyChangeFor("ControlsEnabled")]
		private bool _loadingCategories = true;
		[ObservableProperty]
		[AlsoNotifyChangeFor("ControlsEnabled")]
		private bool _refreshOnErrorButton = false;
		public ObservableCollection<Category> CategoriesList = new ObservableCollection<Category>();
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set => SetProperty(ref _controlsEnabled, value);
		}
		protected bool _internalLoadingCategories;

		public CategoriesViewModel(SettingsService settings, IDispatcherService dispatcher, PlatformService platform, TabsService tabs)
		{
			Settings = settings;
			Dispatcher = dispatcher;
			Platform = platform;
			Tabs = tabs;
			WeakReferenceMessenger.Default.Register(this);
		}

		[ICommand]
		private async Task CategoryClick(GridViewExtParameter item)
		{
			if (item.Item is AddNewCategory)
			{
				var dialog = Platform.CreateDialog<ICreateCategoryDialog>(Dialog.CreateCategory, false);
				var result = await dialog.ShowAsync();
				if (result == IDialogResult.Primary)
				{
					var resultCreate = await CategoriesProvider.CreateCategory(dialog.Name, dialog.Query, dialog.Pin);
					if (resultCreate != null)
					{
						CategoriesList.Add(resultCreate);
						if (string.IsNullOrEmpty(dialog.Query))
							Tabs.OpenTab(Tab.CategoryEdit, resultCreate);
					}
				}
			}
			else
			{
				Tabs.OpenTab(Tab.SearchResults, item.Ctrl, item.Item);
			}
		}

		[ICommand]
		public async Task Refresh()
		{
			if (_internalLoadingCategories)
				return;
			ControlsEnabled = false;
			_internalLoadingCategories = true;
			RefreshOnErrorButton = false;
			LoadingCategories = true;
			CategoriesList.Clear();
			var result = await CategoriesProvider.GetCategories();
			if (result != null)
			{
				if (Settings.Profile.HasApiKey)
					CategoriesList.Add(new AddNewCategory());
				await Task.Run(async () =>
				{
					foreach (var a in result.OrderBy(c => !c.pinned))
						await Dispatcher.RunAsync(() => CategoriesList.Add(a));
				});
			}
			else
				RefreshOnErrorButton = true;
			LoadingCategories = false;
			_internalLoadingCategories = false;
			ControlsEnabled = true;
		}

		public void Receive(DeleteCategoryMessage message)
		{
			CategoriesList.Remove(message.Value);
		}
	}
}
