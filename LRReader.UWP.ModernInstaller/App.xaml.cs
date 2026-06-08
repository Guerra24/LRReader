using LRReader.UWP.Installer.Services;
using LRReader.UWP.Installer.Views;
using Modern.UI.Xaml;

namespace LRReader.UWP.Installer;

public partial class App : XamlApplication
{

	private XamlWindow mainWindow = null!;

	public App()
	{
		this.InitializeComponent();
	}

	protected override void OnLaunched()
	{
		mainWindow = new($"LRReader {Service.AppInfo.Version}", 976, 521)
		{
			MinWidth = 976,
			MinHeight = 521,
			Content = new InstallerPage()
		};
		mainWindow.Activate();
	}
}
