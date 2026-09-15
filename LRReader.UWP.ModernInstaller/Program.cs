using LRReader.UWP.Installer.Services;
using LRReader.UWP.Servicing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices.WindowsRuntime;
using TerraFX.Interop.Windows;
using XamlHostingKit;
using static TerraFX.Interop.Windows.MB;
using static TerraFX.Interop.Windows.Windows;

namespace LRReader.UWP.Installer;

internal partial class Program
{

	[FeatureSwitchDefinition("MrmPatcher.EmbedPri")]
	private static bool EmbedPri => AppContext.TryGetSwitch("MrmPatcher.EmbedPri", out bool embedPri) && embedPri;

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
			using var scope = Service.Services.CreateScope();
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

		//AddPackage("Microsoft.VCLibs.140.00_8wekyb3d8bbwe");
		AddPackage("Microsoft.UI.Xaml.2.8_8wekyb3d8bbwe");

		if (EmbedPri)
		{
			using var stream = Assembly.GetEntryAssembly()!.GetManifestResourceStream("resources.pri")!;
			var data = new byte[stream.Length];
			stream.ReadExactly(data);
			XamlApplication.Start((p) => new App(), data.AsBuffer());
		}
		else
		{
			XamlApplication.Start((p) => new App());
		}

		return 0;
	}

	[UnconditionalSuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
	private static unsafe void AddPackage(string packageFamilyName)
	{
		if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
		{
			fixed (char* pfn = &Utf16StringMarshaller.GetPinnableReference(packageFamilyName))
			{
				char* output;
				var hr = TryCreatePackageDependency(
					null,
					pfn,
					new PACKAGE_VERSION(),
					PackageDependencyProcessorArchitectures.PackageDependencyProcessorArchitectures_None,
					PackageDependencyLifetimeKind.PackageDependencyLifetimeKind_Process,
					null,
					CreatePackageDependencyOptions.CreatePackageDependencyOptions_None, &output);

				if (SUCCEEDED(hr))
				{
					PACKAGEDEPENDENCY_CONTEXT context = new();
					AddPackageDependency(output, 0, AddPackageDependencyOptions.AddPackageDependencyOptions_None, &context, &pfn);
				}
				HeapFree(GetProcessHeap(), 0, output);
			}
		}
		else
		{
			AddDependencyToProcessPackageGraph("Microsoft.UI.Xaml.2.8_8wekyb3d8bbwe");
		}
	}

	[LibraryImport("kernel.appcore.dll", StringMarshalling = StringMarshalling.Utf16)]
	public static partial int AddDependencyToProcessPackageGraph(string packageFamilyName, nint unk = 0, uint unk2 = 0, uint unk3 = 0);

}
