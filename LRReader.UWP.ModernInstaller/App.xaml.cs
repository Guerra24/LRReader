using LRReader.UWP.Installer.Services;
using LRReader.UWP.Installer.Views;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using XamlHostingKit;

namespace LRReader.UWP.Installer;

public partial class App : Application
{

	public App()
	{
		this.InitializeComponent();
	}

	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));

		var window = XamlWindow.Current;

		window.Title = $"LRReader {Service.AppInfo.Version}";

		window.Content = new InstallerPage();

		window.Resize(976, 521);
		window.Show();
	}
}
