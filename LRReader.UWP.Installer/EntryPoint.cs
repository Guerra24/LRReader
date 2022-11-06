using System;
using System.Security.Cryptography.X509Certificates;

namespace LRReader.UWP.Installer
{
	public static class EntryPoint
	{
		[STAThread]
		public static int Main(string[] args)
		{
			if (args != null && args.Length > 0)
			{
				CertUtil.Open(OpenFlags.ReadWrite);
				bool ok = false;
				switch (args[0])
				{
					case "--install-cert":
						ok = CertUtil.InstallCertificate(Variables.CertUrl, Variables.CertThumb).GetAwaiter().GetResult();
						break;
					case "--uninstall-cert":
						ok = CertUtil.UninstallCertificate(Variables.CertThumb);
						break;
				}
				CertUtil.Close();
				return ok ? 0 : -1;
			}
			else
			{
				if (Environment.OSVersion.Version >= new Version(10, 0, 18362, 0))
					UxTheme.SetPreferredAppMode(1);
				var app = new App();
				app.InitializeComponent();
				app.Run();
				return 0;
			}
		}
	}
}
