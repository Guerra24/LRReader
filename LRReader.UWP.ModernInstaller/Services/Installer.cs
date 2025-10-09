using LRReader.UWP.Servicing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Management.Deployment;

namespace LRReader.UWP.Installer.Services;

public class InstallerService
{

	private readonly PackageManager PackageManager;
	private readonly CertUtil CertUtil;
	private readonly AppInfo AppInfo;

	public InstallerService(CertUtil certUtil, AppInfo appInfo)
	{
		PackageManager = new();
		CertUtil = certUtil;
		AppInfo = appInfo;
	}

	public async Task<InstallState> CheckAppState()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

		var certInstalled = CertUtil.FindCertificate(AppInfo.MainCert.Thumbprint);
		var package = PackageManager.FindPackagesForUser(string.Empty, AppInfo.PackageFamilyName).FirstOrDefault();
		if (package != null)
		{
			var version = new Version(package.Id.Version.Major, package.Id.Version.Minor, package.Id.Version.Build, package.Id.Version.Revision);
			return AppInfo.Version > version ? InstallState.UpgradeAvailable : InstallState.Installed;
		}
		else if (certInstalled)
		{
			return InstallState.CertPresent;
		}
		else
		{
			return InstallState.NotInstalled;
		}
	}

	public async Task<InstallResult> Install(IProgress<uint>? progress = null)
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

		if (!CertUtil.FindCertificate(AppInfo.MainCert.Thumbprint))
		{
			var certResult = await ProcessUtil.LaunchAdmin(Environment.ProcessPath!, "--install-cert");
			switch (certResult)
			{
				case -1:
					return new(false, "An invalid certificate has been detected");
				case -99:
					return new(false, "Admin permissions are required for certificate installation");
			}
		}

		var install = PackageManager.AddPackageByAppInstallerFileAsync(AppInfo.AppInstallerUrl, AddPackageByAppInstallerOptions.ForceTargetAppShutdown, PackageManager.GetDefaultPackageVolume());
		install.Progress = (result, prog) =>
		{
			progress?.Report(prog.percentage);
		};
		var result = await install;
		return new(result.IsRegistered, result.ErrorText);
	}

	public async Task Launch()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

		var package = PackageManager.FindPackagesForUser(string.Empty, AppInfo.PackageFamilyName).First();
		var entries = await package.GetAppListEntriesAsync();
		await entries[0].LaunchAsync();
	}

	public async Task Uninstall()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

		var package = PackageManager.FindPackagesForUser(string.Empty, AppInfo.PackageFamilyName).FirstOrDefault();
		if (package != null)
			await PackageManager.RemovePackageAsync(package.Id.FullName);

		await ProcessUtil.LaunchAdmin(Environment.ProcessPath!, "--uninstall-cert");
	}

}

public enum InstallState
{
	NotInstalled, CertPresent, Installed, UpgradeAvailable
}

public record InstallResult(bool IsRegistered, string ErrorText);