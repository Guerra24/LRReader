using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources;

namespace LRReader.UWP.Views.Content.Settings
{

	public sealed partial class Profiles : Page
	{

		private SettingsPageViewModel Data;

		public Profiles()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
			var lang = ResourceLoader.GetForCurrentView("Settings");
			SessionMode.Items.Add(lang.GetString("Profiles/SessionMode/Never"));
			SessionMode.Items.Add(lang.GetString("Profiles/SessionMode/Ask"));
			SessionMode.Items.Add(lang.GetString("Profiles/SessionMode/Always"));
		}

		private async void OpenFolder_Click(object sender, RoutedEventArgs e)
		{
			var ops = new FolderLauncherOptions();
			ops.ItemsToSelect.Add(Data.SettingsManager.ProfilesFile);
			var dir = await Data.SettingsManager.ProfilesFile.GetParentAsync();
			if (dir == null)
				await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(Data.SettingsManager.ProfilesPathLocation), ops);
			else
				await Launcher.LaunchFolderAsync(dir, ops);
		}
	}
}
