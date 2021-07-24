using LRReader.UWP.ViewModels;
using LRReader.UWP.Views.Dialogs;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Main
{

	public sealed partial class FirstRunPage : Page
	{
		private CoreApplicationView CoreView;
		private ApplicationView AppView;

		private FirstRunPageViewModel ViewModel;

		public FirstRunPage()
		{
			this.InitializeComponent();

			CoreView = CoreApplication.GetCurrentView();
			AppView = ApplicationView.GetForCurrentView();
			ViewModel = DataContext as FirstRunPageViewModel;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			TitleBar.Height = CoreView.TitleBar.Height;
			CoreView.TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			CoreView.TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
		}

		private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
		{
			TitleBar.Height = coreTitleBar.Height;
		}

		private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ServerProfileDialog(false);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				var url = dialog.ProfileServerAddress.Text;
				if (!(url.StartsWith("http://") || url.StartsWith("https://")))
					url = "http://" + url;
				ViewModel.SettingsManager.Profile = ViewModel.SettingsManager.AddProfile(dialog.ProfileName.Text, url, dialog.ProfileServerApiKey.Password);
			}
		}

		private async void ButtonEdit_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new ServerProfileDialog(true);
			var profile = ViewModel.SettingsManager.Profile;
			dialog.ProfileName.Text = profile.Name;
			dialog.ProfileServerAddress.Text = profile.ServerAddress;
			dialog.ProfileServerApiKey.Password = profile.ServerApiKey;

			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				var url = dialog.ProfileServerAddress.Text;
				if (!(url.StartsWith("http://") || url.StartsWith("https://")))
					url = "http://" + url;
				ViewModel.SettingsManager.ModifyProfile(profile.UID, dialog.ProfileName.Text, url, dialog.ProfileServerApiKey.Password);
			}
		}

		private void ButtonRemove_Click(object sender, RoutedEventArgs e)
		{
			var sm = ViewModel.SettingsManager;
			sm.Profiles.Remove(sm.Profile);
			sm.Profile = sm.Profiles.FirstOrDefault();
			RemoveFlyout.Hide();
		}

		private void ButtonContinue_Click(object sender, RoutedEventArgs e)
		{
			(Window.Current.Content as Root).Frame.Navigate(typeof(LoadingPage), null, new DrillInNavigationTransitionInfo());
		}
	}
}
