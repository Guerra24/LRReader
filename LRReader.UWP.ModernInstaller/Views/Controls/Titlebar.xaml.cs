using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Installer.Views.Controls;

public sealed partial class Titlebar : UserControl
{
	public Titlebar()
	{
		this.InitializeComponent();
	}

	private void ThemeChanged(FrameworkElement sender, object args)
	{
		((App)Application.Current).OnChangeTheme(ActualTheme == ElementTheme.Dark ? true : false);
	}

}
