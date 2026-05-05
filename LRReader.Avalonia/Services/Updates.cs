using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Avalonia.Controls.ApplicationLifetimes;
using LRReader.Shared.Models;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Services
{

	public class NightlyAppImageUpdatesService : UpdatesService
	{

		public NightlyAppImageUpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings) : base(platform, settingsStorage, settings)
		{
		}

		public override bool CanAutoUpdate => true;

		public override async Task<CheckForUpdatesResult> CheckForUpdates()
		{
			try
			{
				using var client = new HttpClient();

				var version = Version.Parse(await client.GetStringAsync("https://s3.guerra24.net/projects/lrr/linux/nightly/version"));

				return new CheckForUpdatesResult { Result = version > Platform.Version };
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

				var arch = "";

				switch (RuntimeInformation.OSArchitecture)
				{
					case Architecture.X64:
						arch = "x86_64";
						break;
					case Architecture.Arm64:
						arch = "aarch64";
						break;
					default:
						return new UpdateResult { Result = false, ErrorCode = 2, ErrorMessage = "Unsupported architecture... How did you get here?" };
				}

				var hash256 = (await client.GetStringAsync($"https://s3.guerra24.net/projects/lrr/linux/nightly/LRReader.{arch}.AppImage.sha256.txt")).ToLower();

				progress?.Report(0.1);

				var appImage = await client.GetByteArrayAsync($"https://s3.guerra24.net/projects/lrr/linux/nightly/LRReader.{arch}.AppImage");

				progress?.Report(0.4);

				var localHash256 = Convert.ToHexStringLower(SHA256.HashData(appImage));

				if (hash256 != localHash256)
					return new UpdateResult { Result = false, ErrorCode = 1, ErrorMessage = $"Hash mismatch. Got {localHash256}, expected {hash256}." };

				progress?.Report(0.6);

				var path = Environment.GetEnvironmentVariable("APPIMAGE");

				if (string.IsNullOrEmpty(path))
					return new UpdateResult { Result = false, ErrorCode = 3, ErrorMessage = "Unable to update AppImage" };

				await File.WriteAllBytesAsync(path, appImage);

				progress?.Report(1.0);

				Process.Start(path);

				((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Shutdown();

				return new UpdateResult { Result = true };
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
				return new UpdateResult { Result = false, ErrorCode = e.HResult, ErrorMessage = Platform.GetLocalizedString("/Shared/Updater/UpdateError") };
			}
		}
	}
}
