using LRReader.Shared.Models;
using LRReader.Shared.Services;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.Services.Store;

namespace LRReader.UWP.Services
{

	public class StoreUpdatesService : UpdatesService
	{

		private StoreContext Context;
		private Package Current;

		public StoreUpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings) : base(platform, settingsStorage, settings)
		{
			Context = StoreContext.GetDefault();
			Current = Package.Current;
		}

		public override bool CanAutoUpdate()
		{
			try
			{
				return Context.CanSilentlyDownloadStorePackageUpdates;
			}
			catch (Exception e)
			{
				Crashes.TrackError(e);
			}
			return false;
		}

		public override async Task<CheckForUpdatesResult> CheckForUpdates()
		{
			if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable || NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
				return new CheckForUpdatesResult { Found = false };
			try
			{
				var result = new CheckForUpdatesResult();
				var packageUpdates = await Context.GetAppAndOptionalStorePackageUpdatesAsync();
				result.Found = packageUpdates.Count > 0;
				var pack = packageUpdates.FirstOrDefault(p => p.Package.Id.FamilyName.Equals(Current.Id.FamilyName));
				if (pack != null)
				{
					var ver = pack.Package.Id.Version;
					result.Target = new Version(ver.Major, ver.Minor, ver.Build, 0); // Rev is always 0
				}
				return result;
			}
			catch (Exception e)
			{
				Crashes.TrackError(e);
			}
			return new CheckForUpdatesResult { Found = false };
		}

		public override async Task<UpdateResult> DownloadAndInstall(IProgress<double> progress)
		{
			if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable || NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
				return new UpdateResult { Result = false, ErrorCode = -1, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/NoNetwork") };
			try
			{
				var packageUpdates = await Context.GetAppAndOptionalStorePackageUpdatesAsync();
				if (packageUpdates.Count == 0)
					return new UpdateResult { Result = false, ErrorCode = -1, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/NotFound") };

				IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> downloadTask;
				if (CanAutoUpdate())
					downloadTask = Context.TrySilentDownloadAndInstallStorePackageUpdatesAsync(packageUpdates);
				else
					downloadTask = Context.RequestDownloadAndInstallStorePackageUpdatesAsync(packageUpdates);
				downloadTask.Progress = (info, prog) => progress?.Report(prog.TotalDownloadProgress);
				var result = await downloadTask.AsTask();
				return new UpdateResult { Result = result.OverallState == StorePackageUpdateState.Completed };
			}
			catch (Exception e)
			{
				Crashes.TrackError(e);
				return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/UpdateError") };
			}
		}
	}

	public class SideloadUpdatesService : UpdatesService
	{
		private readonly ILogger<SideloadUpdatesService> Logger;

		private Package Current;

		public SideloadUpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings, ILogger<SideloadUpdatesService> logger) : base(platform, settingsStorage, settings)
		{
			Logger = logger;
			Current = Package.Current;
		}

		public override bool CanAutoUpdate() => true;

		public override async Task<CheckForUpdatesResult> CheckForUpdates()
		{
			Logger.LogInformation("Checking for updates");
			/*
			var rq = new RestRequest("lrr/upgrade/check"); // Implement
			rq.AddParameter("version", Platform.Version.ToString());

			var r = await client.ExecuteGetAsync(rq);

			var result = await r.GetResultInternal<bool>();

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				return new CheckForUpdatesResult { Found = false }; ;
			}
			if (result.OK)
			{
				return new CheckForUpdatesResult { Found = false };
			}*/
			try
			{
				// We can't use Current here cause it crashes on 1809...
				var pm = new PackageManager();
				var package = pm.FindPackageForUser(string.Empty, Current.Id.FullName);
				var result = await package.CheckUpdateAvailabilityAsync();
				Logger.LogInformation("Result: {0}", result.Availability);
				return new CheckForUpdatesResult { Found = result.Availability == PackageUpdateAvailability.Available || result.Availability == PackageUpdateAvailability.Required };
			}
			catch (Exception e)
			{
				Crashes.TrackError(e);
				Logger.LogError("Thrown exception: {0}\n{1}", e.Message, e.StackTrace);
				return new CheckForUpdatesResult { Found = false };
			}
		}

		public override async Task<UpdateResult> DownloadAndInstall(IProgress<double> progress)
		{
			Logger.LogInformation("Download and install");
			try
			{
				Logger.LogInformation("Source: {0}", Current.GetAppInstallerInfo().Uri);
				var pm = new PackageManager();
				var downloadTask = pm.AddPackageByAppInstallerFileAsync(Current.GetAppInstallerInfo().Uri, AddPackageByAppInstallerOptions.ForceTargetAppShutdown, pm.GetDefaultPackageVolume());
				downloadTask.Progress = (info, prog) => progress?.Report(prog.percentage / 100d);
				var result = await downloadTask.AsTask();
				return new UpdateResult { Result = result.IsRegistered };
			}
			catch (Exception e)
			{
				Crashes.TrackError(e);
				Logger.LogError("Thrown exception: {0}\n{1}", e.Message, e.StackTrace);
				return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/UpdateError") };
			}
		}
	}
}
