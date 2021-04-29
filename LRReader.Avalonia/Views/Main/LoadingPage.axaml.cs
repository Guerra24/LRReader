using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LRReader.Shared.Extensions;
using LRReader.Shared.Internal;
using LRReader.Shared.Providers;
using LRReader.Shared.ViewModels;
using System;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Avalonia.Views.Main
{
	public class LoadingPage : UserControl
	{
		private LoadingPageViewModel ViewModel;

		private ResourceLoader lang;

		public LoadingPage()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			ViewModel = DataContext as LoadingPageViewModel;
			lang = ResourceLoader.GetForCurrentView("Pages");
		}

		private async void LoadingPage_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
		{
			if (Design.IsDesignMode)
				return;
			await InitServices();

			bool firstRun = Settings.Profile == null;
			if (firstRun)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(500));
				(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.Content = new FirstRunPage();
				return;
			}
			ViewModel.Active = true;
#if !DEBUG
			await SharedGlobal.UpdatesManager.UpdateSupportedRange(Platform.GetVersion());
#endif
			Api.RefreshSettings(Settings.Profile);
			var serverInfo = await ServerProvider.GetServerInfo();
			if (serverInfo == null)
			{
				var address = Settings.Profile.ServerAddress;
				if (address.Contains("127.0.0.") || address.Contains("localhost"))
				{
					ViewModel.Status = lang.GetString("LoadingPage/NoConnectionLocalHost");
					ViewModel.StatusSub = lang.GetString("LoadingPage/NoConnectionLocalHostSub");
				}
				else
					ViewModel.Status = lang.GetString("LoadingPage/NoConnection");
				await Reload();
				return;
			}
			else if (serverInfo._unauthorized)
			{
				ViewModel.Status = lang.GetString("LoadingPage/InvalidKey");
				await Reload();
				return;
			}
			Api.ServerInfo = serverInfo;
			if (serverInfo.version < UpdatesManager.MIN_VERSION)
			{
				ViewModel.Status = lang.GetString("LoadingPage/InstanceNotSupported").AsFormat(serverInfo.version, UpdatesManager.MIN_VERSION);
				await Reload();
				return;
			}
			else if (serverInfo.version > UpdatesManager.MAX_VERSION)
			{
				ViewModel.Status = lang.GetString("LoadingPage/ClientNotSupported").AsFormat(serverInfo.version);
				ViewModel.StatusSub = lang.GetString("LoadingPage/ClientRange").AsFormat(UpdatesManager.MIN_VERSION, UpdatesManager.MAX_VERSION);
				await Reload();
				return;
			}
			if (serverInfo.nofun_mode && !Settings.Profile.HasApiKey)
			{
				ViewModel.Status = lang.GetString("LoadingPage/MissingKey");
				await Reload();
				return;
			}
			await Archives.ReloadArchives();
			Api.ControlFlags.Check(serverInfo);
			ViewModel.Active = false;
			(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.Content = new HostTabPage();

		}

		private async Task Reload()
		{
			ViewModel.Active = false;
			await Task.Delay(TimeSpan.FromSeconds(5));
			ViewModel.Status = "";
			ViewModel.StatusSub = "";
			(Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.Content = new FirstRunPage();
		}
	}
}
