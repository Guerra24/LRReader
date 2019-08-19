using LRReader.Internal;
using LRReader.ViewModels;
using LRReader.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views.Main
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class FirstRun : Page
	{
		private SettingsPageViewModel Data;

		public FirstRun()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			Frame.BackStack.Clear();
			Global.EventManager.ShowHeader(true);
		}

		private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			ServerProfileDialog dialog = new ServerProfileDialog(false);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.AddProfile(dialog.ProfileName.Text, dialog.ProfileServerAddress.Text, dialog.ProfileServerApiKey.Password);
			}
		}

		private async void ButtonEdit_Click(object sender, RoutedEventArgs e)
		{
			ServerProfileDialog dialog = new ServerProfileDialog(true);
			ServerProfile profile = Data.SettingsManager.Profile;
			dialog.ProfileName.Text = profile.Name;
			dialog.ProfileServerAddress.Text = profile.ServerAddress;
			dialog.ProfileServerApiKey.Password = profile.ServerApiKey;

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				Data.SettingsManager.ModifyProfile(profile.UID, dialog.ProfileName.Text, dialog.ProfileServerAddress.Text, dialog.ProfileServerApiKey.Password);
			}
		}

		private void ButtonContinue_Click(object sender, RoutedEventArgs e)
		{
			Global.EventManager.ShowHeader(true);
			Frame.Navigate(typeof(ArchivesPage), new EntranceNavigationTransitionInfo());
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Global.LRRApi.RefreshSettings();
		}

	}
}
