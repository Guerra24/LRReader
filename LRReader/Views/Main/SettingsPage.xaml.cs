using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views.Main
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SettingsPage : Page
	{
		public SettingsPage()
		{
			this.InitializeComponent();
		}

		private void ApplyCntBtn_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(Server.Text))
			{
				ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
				if (Uri.IsWellFormedUriString(Server.Text, UriKind.Absolute))
				{
					roamingSettings.Values["ServerAddress"] = Server.Text;
					if (!string.IsNullOrEmpty(ApiKey.Password))
						roamingSettings.Values["ApiKey"] = ApiKey.Password;
					else
						roamingSettings.Values["ApiKey"] = "";
					Global.LRRApi.RefreshSettings();
				} else
				{
					Global.EventManager.ShowError("Connection Error", "Invalid address");
				}
			}
			else
			{
				Global.EventManager.ShowError("Connection Error", "Empty server address");
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
			Server.Text = roamingSettings.Values["ServerAddress"] as string;
			ApiKey.Password = roamingSettings.Values["ApiKey"] as string;
		}

		private void ResetCntBtn_Click(object sender, RoutedEventArgs e)
		{
			ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
			Server.Text = roamingSettings.Values["ServerAddress"] as string;
			ApiKey.Password = roamingSettings.Values["ApiKey"] as string;
		}
	}
}
