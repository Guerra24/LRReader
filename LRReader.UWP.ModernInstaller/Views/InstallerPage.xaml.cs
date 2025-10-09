using LRReader.UWP.Installer.Services;
using LRReader.UWP.Installer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Installer.Views;

public sealed partial class InstallerPage : UserControl
{
	public InstallerPageViewModel Data;

	public InstallerPage()
	{
		this.InitializeComponent();
		if (Environment.OSVersion.Version >= new Version(10, 0, 22621, 0))
			Root.Background = new SolidColorBrush(Colors.Transparent);
		Data = Service.Services.GetRequiredService<InstallerPageViewModel>();
	}

	private async void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LRReader.UWP.ModernInstaller.logo.ico"))
		{
			var bitmap = new BitmapImage();
			await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
			Logo.Source = bitmap;
		}
		await Data.Load();
	}
}
