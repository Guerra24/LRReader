﻿using CommunityToolkit.WinUI.Helpers;
using LRReader.Shared;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using RestSharp;
using Sentry;
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
				SentrySdk.CaptureException(e);
			}
			return false;
		}

		public override async Task<CheckForUpdatesResult> CheckForUpdates()
		{
			if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable || NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
				return new CheckForUpdatesResult { Result = false };
			try
			{
				var result = new CheckForUpdatesResult();
				var packageUpdates = await Context.GetAppAndOptionalStorePackageUpdatesAsync();
				result.Result = packageUpdates.Count > 0;
				var pack = packageUpdates.FirstOrDefault(p => p.Package.Id.FamilyName.Equals(Current.Id.FamilyName));
				if (pack != null)
				{
					var rq = new RestRequest("lrr/upgrade/check");
					var r = await client.ExecuteGetAsync(rq);
					var updatesResult = await r.GetResultInternal<CheckForUpdatesResult>();
					if (updatesResult.OK && updatesResult.Data != null)
						return updatesResult.Data;
				}
				return result;
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
				return new CheckForUpdatesResult { Result = false, ErrorCode = e.HResult, ErrorMessage = e.Message };
			}
		}

		public override async Task<UpdateResult> DownloadAndInstall(IProgress<double> progress, CheckForUpdatesResult? check = null)
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
				SentrySdk.CaptureException(e);
				return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/UpdateError") };
			}
		}
	}

	public class SideloadUpdatesService : UpdatesService
	{

		public SideloadUpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings) : base(platform, settingsStorage, settings)
		{
		}

		public override bool CanAutoUpdate() => true;

		public override async Task<CheckForUpdatesResult> CheckForUpdates()
		{
			var rq = new RestRequest("lrr/upgrade/check");
			rq.AddParameter("version", Platform.Version.ToString());
			var r = await client.ExecuteGetAsync(rq);
			var updatesResult = await r.GetResultInternal<CheckForUpdatesResult>();
			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				return new CheckForUpdatesResult { Result = false };
			}
			if (updatesResult.OK && updatesResult.Data != null)
			{
				return updatesResult.Data;
			}
			return new CheckForUpdatesResult { Result = false };
		}

		public override async Task<UpdateResult> DownloadAndInstall(IProgress<double> progress, CheckForUpdatesResult? check = null)
		{
			if (check == null)
			{
				return new UpdateResult { Result = false, ErrorCode = -1, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/UpdateError") };
			}
			try
			{
				var pm = new PackageManager();
				var downloadTask = pm.AddPackageByAppInstallerFileAsync(new Uri(check.Link), AddPackageByAppInstallerOptions.ForceTargetAppShutdown, pm.GetDefaultPackageVolume());
				downloadTask.Progress = (info, prog) => progress?.Report(prog.percentage / 100d);
				var result = await downloadTask.AsTask();
				if (result.IsRegistered)
					return new UpdateResult { Result = true };
				else
					return new UpdateResult { Result = false, ErrorCode = result.ExtendedErrorCode.HResult, ErrorMessage = result.ErrorText };
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
				return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/UpdateError") };
			}
		}
	}

	public class NightlyUpdatesService : UpdatesService
	{

		private Package Current;

		public NightlyUpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings) : base(platform, settingsStorage, settings)
		{
			Current = Package.Current;
		}

		public override bool CanAutoUpdate() => true;

		public override async Task<CheckForUpdatesResult> CheckForUpdates()
		{
			try
			{
				// We can't use Current here cause it crashes on 1809...
				var pm = new PackageManager();
				var package = pm.FindPackageForUser(string.Empty, Current.Id.FullName);
				var result = await package.CheckUpdateAvailabilityAsync();
				return new CheckForUpdatesResult { Result = result.Availability == PackageUpdateAvailability.Available || result.Availability == PackageUpdateAvailability.Required };
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
				return new CheckForUpdatesResult { Result = false, ErrorCode = e.HResult, ErrorMessage = e.Message };
			}
		}

		public override async Task<UpdateResult> DownloadAndInstall(IProgress<double> progress, CheckForUpdatesResult? check = null)
		{
			try
			{
				var pm = new PackageManager();
				var downloadTask = pm.AddPackageByAppInstallerFileAsync(Current.GetAppInstallerInfo().Uri, AddPackageByAppInstallerOptions.ForceTargetAppShutdown, pm.GetDefaultPackageVolume());
				downloadTask.Progress = (info, prog) => progress?.Report(prog.percentage / 100d);
				var result = await downloadTask.AsTask();
				if (result.IsRegistered)
					return new UpdateResult { Result = true };
				else
					return new UpdateResult { Result = false, ErrorCode = result.ExtendedErrorCode.HResult, ErrorMessage = result.ErrorText };
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
				return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/UpdateError") };
			}
		}
	}
}
