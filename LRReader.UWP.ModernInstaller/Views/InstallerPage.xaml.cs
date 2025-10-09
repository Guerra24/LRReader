using LRReader.UWP.Installer.Services;
using LRReader.UWP.Installer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Installer.Views;

public sealed partial class InstallerPage : Page
{
	public InstallerPageViewModel Data;

	public InstallerPage()
	{
		this.InitializeComponent();
		Data = Service.Services.GetRequiredService<InstallerPageViewModel>();
	}

	private async void Page_Loaded(object sender, RoutedEventArgs e)
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
