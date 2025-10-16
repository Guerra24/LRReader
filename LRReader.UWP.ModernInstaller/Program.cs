using LRReader.UWP.Installer.Services;
using LRReader.UWP.Servicing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.MB;
using static TerraFX.Interop.Windows.Windows;

namespace LRReader.UWP.Installer;

internal class Program
{
	private static App xamlApp = null!;

	[STAThread]
	public static unsafe int Main(string[] args)
	{
		if (Environment.OSVersion.Version < new Version(10, 0, 19041, 0))
		{
			var title = (char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "Not supported".GetPinnableReference()));
			var content = (char*)Unsafe.AsPointer(ref Unsafe.AsRef(in "LRReader requires Windows 10 20H1 or newer".GetPinnableReference()));

			MessageBoxW(HWND.NULL, content, title, MB_ICONERROR | MB_OK);
			return 0;
		}

		if (!Version.TryParse("{APP_VERSION}", out var version))
			version = new(0, 0, 0, 0);

		if (!Uri.TryCreate("{APP_INSTALLER_URL}", UriKind.Absolute, out var appInstallerUri))
			appInstallerUri = new("https://s3.guerra24.net/projects/lrr/nightly/LRReader.UWP.appinstaller");

		Service.BuildServices(new AppInfo(
			"Guerra24.LRReader_3fr0p4qst6948",
			appInstallerUri,
			new CertMeta(new Uri(CertInfo.CertUrlV2), CertInfo.CertThumbV2),
			[CertInfo.CertThumb],
			version));

		var appInfo = Service.AppInfo;
		if (args != null && args.Length > 0)
		{
			using (var scope = Service.Services.CreateScope())
			{
				var certUtil = scope.ServiceProvider.GetRequiredService<CertUtil>();
				bool ok = false;
				switch (args[0])
				{
					case "--install-cert":
						ok = certUtil.InstallCertificate(appInfo.MainCert.Url, appInfo.MainCert.Thumbprint).GetAwaiter().GetResult();
						break;
					case "--uninstall-cert":
						ok = certUtil.UninstallCertificate(appInfo.MainCert.Thumbprint);
						foreach (var certThumb in appInfo.ExpiredCerts)
							ok = certUtil.UninstallCertificate(certThumb);
						break;
				}
				return ok ? 0 : -1;
			}
		}

		xamlApp = new();
		xamlApp.Run();
		return 0;
	}

}
