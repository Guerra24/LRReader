using System.Windows;
using LRReader.UWP.Servicing;

namespace LRReader.UWP.Installer
{
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e) => LegacyCertUtil.Open();

		private void Application_Exit(object sender, ExitEventArgs e) => LegacyCertUtil.Close();
	}
}
