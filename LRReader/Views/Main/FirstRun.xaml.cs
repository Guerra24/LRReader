using LRReader.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace LRReader.Views.Main
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class FirstRun : Page
	{
		public FirstRun()
		{
			this.InitializeComponent();
		}

		private void AcceptBtn_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(Server.Text))
			{
				if (Uri.IsWellFormedUriString(Server.Text, UriKind.Absolute))
				{
					Global.LRRApi.RefreshSettings();
					Frame.Navigate(typeof(ArchivesPage), new DrillInNavigationTransitionInfo());
				}
				else
				{
					Global.EventManager.ShowError("Connection Error", "Invalid address");
				}
			}
			else
			{
				Global.EventManager.ShowError("Connection Error", "Empty server address");
			}
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			Frame.BackStack.Clear();
			Global.EventManager.ShowHeader(true);
		}
	}
}
