using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{

	public sealed partial class Profiles : Page
	{

		private SettingsPageViewModel Data;

		public Profiles()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<SettingsPageViewModel>();
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
