using LRReader.Host.Impl;
using LRReader.Shared.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LRReader.Host
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Init.InitObjects();
			(SharedGlobal.SettingsStorage as SettingsStorage).Load();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			(SharedGlobal.SettingsStorage as SettingsStorage).Save();
		}
	}
}
