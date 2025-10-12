using LRReader.UWP.Installer.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Installer.Views.Controls;

public sealed partial class Titlebar : UserControl
{
	public Titlebar()
	{
		this.InitializeComponent();
		Title.Text = $"LRReader {Service.AppInfo.Version.ToString()}";
	}

	private void ThemeChanged(FrameworkElement sender, object args)
	{
		((App)Application.Current).OnChangeTheme(ActualTheme == ElementTheme.Dark ? true : false);
	}

}
