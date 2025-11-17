using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views
{
	public sealed partial class Root : Page
	{
		public Root()
		{
			this.InitializeComponent();
		}

		public void ChangeTheme(AppTheme theme) => RequestedTheme = theme.ToXamlTheme();

		public void UpdateThemeColors()
		{
			var AppView = ApplicationView.GetForCurrentView();
			var titleBar = AppView.TitleBar;
			switch (ActualTheme)
			{
				case ElementTheme.Light:
					titleBar.ButtonForegroundColor = Colors.Black;
					break;
				case ElementTheme.Dark:
					titleBar.ButtonForegroundColor = Colors.White;
					break;
			}
		}
	}
}
