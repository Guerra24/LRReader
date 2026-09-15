using LRReader.UWP.Installer.Services;
using LRReader.UWP.Installer.ViewModels;
using LRReader.UWP.ModernInstaller.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using XamlHostingKit;

namespace LRReader.UWP.Installer.Views;

public sealed partial class InstallerPage : Page
{
	public InstallerPageViewModel Data;

	public InstallerPage()
	{
		this.InitializeComponent();
		Data = Service.Services.GetRequiredService<InstallerPageViewModel>();
	}

	private async void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LRReader.UWP.ModernInstaller.icon.ico"))
		{
			var bitmap = new BitmapImage();
			await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
			Logo.Source = bitmap;
		}
		await Data.Load();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		XamlApplication.CreateNewWindow(new WindowCreationOptions { Title = "Settings" }, (p) =>
		{
			SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));

			var window = XamlWindow.Current;

			window.Content = new SettingsPage();

			window.Show();
		});
	}
}
