using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace LRReader.Shared.ViewModels.Base
{
	public partial class CategoryBaseViewModel : ObservableObject
	{
		private readonly PlatformService Platform;
		private readonly TabsService Tabs;
		protected readonly SettingsService Settings;

		[AllowNull]
		[ObservableProperty]
		private Category _category;
		[ObservableProperty]
		private bool _missingImage = false;
		[ObservableProperty]
		private bool _searchImage = false;

		public bool CanEdit => Settings.Profile.HasApiKey;

		public CategoryBaseViewModel(PlatformService platform, TabsService tabs, SettingsService settings)
		{
			Platform = platform;
			Tabs = tabs;
			Settings = settings;
		}

		[RelayCommand]
		private void OpenTab() => Tabs.OpenTab(Tab.SearchResults, false, Category);

		[RelayCommand]
		private async Task Edit()
		{
			var listMode = string.IsNullOrEmpty(Category.search);

			if (Category.Unconfigured())
			{
				var result = await Platform.OpenGenericDialog(
					Platform.GetLocalizedString("Dialogs/ConfigureCategory/Title"),
					Platform.GetLocalizedString("Dialogs/ConfigureCategory/PrimaryButtonText"),
					Platform.GetLocalizedString("Dialogs/ConfigureCategory/SecondaryButtonText"),
					Platform.GetLocalizedString("Dialogs/ConfigureCategory/CloseButtonText"),
					Platform.GetLocalizedString("Dialogs/ConfigureCategory/Content").AsFormat("\n"));

				switch (result)
				{
					case IDialogResult.Primary:
						listMode = true;
						break;
					case IDialogResult.Secondary:
						listMode = false;
						break;
				}
			}

			if (listMode)
				Tabs.OpenTab(Tab.CategoryEdit, Category);
			else
			{
				var dialog = Platform.CreateDialog<ICreateCategoryDialog>(Dialog.CreateCategory, true);
				dialog.Name = Category.name;
				dialog.Query = Category.search;
				dialog.Pin = Category.pinned;
				var result = await dialog.ShowAsync();
				if (result == IDialogResult.Primary)
				{
					var updateResult = await CategoriesProvider.UpdateCategory(Category.id, dialog.Name, dialog.Query, dialog.Pin);
					if (updateResult)
					{
						Category.name = dialog.Name;
						Category.search = dialog.Query;
						Category.pinned = dialog.Pin;
						OnPropertyChanged("Category");
					}
				}
			}
		}

		[RelayCommand]
		private async Task Delete()
		{
			var result = await Platform.OpenGenericDialog(
					Platform.GetLocalizedString("Dialogs/RemoveCategory/Title").AsFormat(Category.name),
					Platform.GetLocalizedString("Dialogs/RemoveCategory/PrimaryButtonText"),
					closebutton: Platform.GetLocalizedString("Dialogs/RemoveCategory/CloseButtonText"),
					content: Platform.GetLocalizedString("Dialogs/RemoveCategory/Content")
				);
			if (result == IDialogResult.Primary)
			{
				WeakReferenceMessenger.Default.Send(new DeleteCategoryMessage(Category));
				Tabs.CloseTabWithId("Search_" + Category.id);
				await CategoriesProvider.DeleteCategory(Category.id);
			}
		}
	}
}
