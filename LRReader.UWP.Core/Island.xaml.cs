using LRReader.Internal;
using LRReader.UWP.Views.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.UWP.Core
{
	public sealed partial class Island : UserControl
	{
		public Island()
		{
			this.InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			/*bool firstRun = Global.SettingsManager.Profile == null;
			if (firstRun)
			{
				Root.Navigate(typeof(FirstRunPage));
			}
			else
			{
				Root.Navigate(typeof(HostTabPage));
			}*/
		}
	}
}
