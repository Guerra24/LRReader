using LRReader.Internal;
using LRReader.ViewModels;
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
		private SettingsPageViewModel Data;

		public SettingsPage()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			if (!string.IsNullOrEmpty(Data.SettingsManager.ServerAddress))
			{
				if (Uri.IsWellFormedUriString(Data.SettingsManager.ServerAddress, UriKind.Absolute))
				{
					Global.LRRApi.RefreshSettings();
				}
				else
				{
					e.Cancel = true;
					Global.EventManager.ShowError("Connection Error", "Invalid address");
				}
			}
			else
			{
				e.Cancel = true;
				Global.EventManager.ShowError("Connection Error", "Empty server address");
			}
		}
	}
}
