using LRReader.Shared.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using TenMica;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
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
			if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13) && AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
			{
				Background = new TenMicaBrush();
			}
			else
			{
				BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
			}
		}

		public void ChangeTheme(AppTheme theme)
		{
			switch (theme)
			{
				case AppTheme.System:
					RequestedTheme = ElementTheme.Default;
					if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13) && AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
						((TenMicaBrush)Background).ThemeForced = false;
					break;
				case AppTheme.Dark:
					RequestedTheme = ElementTheme.Dark;
					if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13) && AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
						((TenMicaBrush)Background).ForcedTheme = ApplicationTheme.Dark;
					break;
				case AppTheme.Light:
					RequestedTheme = ElementTheme.Light;
					if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13) && AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
						((TenMicaBrush)Background).ForcedTheme = ApplicationTheme.Light;
					break;
			}
		}

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
