using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LRReader.Shared.Extensions;
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;

namespace LRReader.Shared.ViewModels.Base
{
	public partial class TankoubonBaseViewModel : ObservableObject
	{
		private readonly PlatformService Platform;
		private readonly TabsService Tabs;
		protected readonly SettingsService Settings;

		[ObservableProperty]
		private Tankoubon _tankoubon = null!;
		[ObservableProperty]
		private bool _missingImage;
		[ObservableProperty]
		private bool _searchImage;

		public bool CanEdit => Settings.Profile.HasApiKey;

		public TankoubonBaseViewModel(PlatformService platform, TabsService tabs, SettingsService settings)
		{
			Platform = platform;
			Tabs = tabs;
			Settings = settings;
		}

		[RelayCommand]
		private void OpenTab()
		{
			Tabs.OpenTab(Tab.Tankoubon, false, Tankoubon);
		}

		[RelayCommand]
		private void Edit()
		{
			Tabs.OpenTab(Tab.TankoubonEdit, Tankoubon);
		}

		[RelayCommand]
		private async Task Delete()
		{
			var result = await Platform.OpenGenericDialog(
					Platform.GetLocalizedString("Dialogs/RemoveTankoubon/Title").AsFormat(Tankoubon.name),
					Platform.GetLocalizedString("Dialogs/RemoveTankoubon/PrimaryButtonText"),
					closebutton: Platform.GetLocalizedString("Dialogs/RemoveTankoubon/CloseButtonText"),
					content: Platform.GetLocalizedString("Dialogs/RemoveTankoubon/Content")
				);
			if (result == IDialogResult.Primary)
			{
				WeakReferenceMessenger.Default.Send(new DeleteTankoubonMessage(Tankoubon));
				Tabs.CloseTabWithId("Tankoubon_" + Tankoubon.id);
				await TankoubonsProvider.DeleteTankoubon(Tankoubon.id);
			}
		}
	}
}
