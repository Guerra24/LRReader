using LRReader.UWP.Installer.Services;
using LRReader.UWP.Installer.Views;
using Modern.UI.Xaml;

namespace LRReader.UWP.Installer;

public partial class App : XamlApplication
{

	private XamlWindow mainWindow = null!;

	public App()
	{
		InitializeComponent();
	}

	protected override void OnLaunched()
	{
		mainWindow = new($"LRReader {Service.AppInfo.Version}", 976, 521);
		mainWindow.MinWidth = 976;
		mainWindow.MinHeight = 521;
		mainWindow.Content = new InstallerPage();
		mainWindow.Activate();
	}
}
