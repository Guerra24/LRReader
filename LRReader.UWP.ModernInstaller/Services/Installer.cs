using LRReader.UWP.Servicing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;

namespace LRReader.UWP.Installer.Services;

public class InstallerService
{

	private readonly PackageManager PackageManager;
	private readonly AppInfo AppInfo;

	public InstallerService(AppInfo appInfo)
	{
		PackageManager = new();
		AppInfo = appInfo;
	}

	public async Task<InstallState> CheckAppState()
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		using (var scope = Service.Services.CreateScope())
		{
			var certUtil = scope.ServiceProvider.GetRequiredService<CertUtil>();

			var certInstalled = certUtil.FindCertificate(AppInfo.MainCert.Thumbprint);
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
	}

	public async Task<InstallResult> Install(IProgress<DeploymentProgress>? progress = null)
	{
		await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
		using (var scope = Service.Services.CreateScope())
		{
			var certUtil = scope.ServiceProvider.GetRequiredService<CertUtil>();

			if (!certUtil.FindCertificate(AppInfo.MainCert.Thumbprint))
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
		}
		using var cts = new CancellationTokenSource();
		var result = await PackageManager.AddPackageByAppInstallerFileAsync(AppInfo.AppInstallerUrl, AddPackageByAppInstallerOptions.ForceTargetAppShutdown, PackageManager.GetDefaultPackageVolume()).AsTask(cts.Token, progress); ;
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