using Android.Content;
using Android.Content.PM;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using System.Security.Cryptography;

namespace LRReader.Avalonia.Android.Services
{

	public class NightlyAndroidUpdatesService : UpdatesService
	{

		public NightlyAndroidUpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings) : base(platform, settingsStorage, settings)
		{
		}

		public override bool CanAutoUpdate => true;

		public override async Task<CheckForUpdatesResult> CheckForUpdates()
		{
			try
			{
				using var client = new HttpClient();

				var version = long.Parse(await client.GetStringAsync("https://s3.guerra24.net/projects/lrr/android/nightly/version"));

				var context = global::Android.App.Application.Context;
				long build;
				if (OperatingSystem.IsAndroidVersionAtLeast(28))
					build = context.PackageManager!.GetPackageInfo(context.PackageName!, 0)!.LongVersionCode;
				else
					build = context.PackageManager!.GetPackageInfo(context.PackageName!, 0)!.VersionCode;
				return new CheckForUpdatesResult { Result = version > build };
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
				using var client = new HttpClient();

				var hash256 = (await client.GetStringAsync($"https://s3.guerra24.net/projects/lrr/android/nightly/sha256.txt")).Trim().ToLower();

				progress?.Report(0.1);

				var apk = await client.GetByteArrayAsync($"https://s3.guerra24.net/projects/lrr/android/nightly/net.guerra24.lrreader.nightly-Signed.apk");

				progress?.Report(0.4);

				var localHash256 = Convert.ToHexStringLower(SHA256.HashData(apk));

				if (hash256 != localHash256)
					return new UpdateResult { Result = false, ErrorCode = 1, ErrorMessage = $"Hash mismatch. Got {localHash256}, expected {hash256}." };

				progress?.Report(0.6);

				UpdateReceiver.UpdateCompleted = new();

				var context = global::Android.App.Application.Context;

				var packageInstaller = context.PackageManager!.PackageInstaller!;

				var session = packageInstaller.OpenSession(packageInstaller.CreateSession(new PackageInstaller.SessionParams(PackageInstallMode.FullInstall)));

				try
				{
					using (var stream = session.OpenWrite(context.PackageName!, 0, apk.LongLength))
					{
						await stream.WriteAsync(apk);
						session.Fsync(stream);
					}

					var intent = new Intent(context, typeof(UpdateReceiver));

					var flags = PendingIntentFlags.UpdateCurrent;
					if (OperatingSystem.IsAndroidVersionAtLeast(31))
						flags |= PendingIntentFlags.Mutable;

					var pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, flags);

					session.Commit(pendingIntent!.IntentSender);

					progress?.Report(0.9);

					session.Close();

					var result = await UpdateReceiver.UpdateCompleted.Task;

					progress?.Report(1.0);

					return new UpdateResult { Result = result.Result, ErrorMessage = result.Message };
				}
				catch (Exception e)
				{
					session.Abandon();
					session.Close();
					SentrySdk.CaptureException(e);
					return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("Shared/Updater/UpdateError") };
				}
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
				return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("Shared/Updater/UpdateError") };
			}
		}
	}

	[BroadcastReceiver(Enabled = true)]
	public class UpdateReceiver : BroadcastReceiver
	{
		public static TaskCompletionSource<AndroidUpdateResult> UpdateCompleted { get; set; } = null!;

		public override void OnReceive(Context? context, Intent? intent)
		{
			switch ((PackageInstallStatus)intent!.GetIntExtra(PackageInstaller.ExtraStatus, -1))
			{
				case PackageInstallStatus.PendingUserAction:
					Intent activityIntent;
					if (OperatingSystem.IsAndroidVersionAtLeast(33))
						activityIntent = (Intent)intent!.GetParcelableExtra(Intent.ExtraIntent, Java.Lang.Class.FromType(typeof(Intent)))!;
					else
						activityIntent = (Intent)intent!.GetParcelableExtra(Intent.ExtraIntent)!;

					context!.StartActivity(activityIntent.AddFlags(ActivityFlags.NewTask));
					break;
				case PackageInstallStatus.Success:
					UpdateCompleted.SetResult(new AndroidUpdateResult(true));
					break;
				default:
					var msg = intent!.GetStringExtra(PackageInstaller.ExtraStatusMessage);
					UpdateCompleted.SetResult(new AndroidUpdateResult(false, msg ?? ""));
					break;
			}
		}
	}

	public record AndroidUpdateResult(bool Result, string Message = "");
}
