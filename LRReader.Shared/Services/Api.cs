using LRReader.Shared.Models.Main;
#if WINDOWS_UWP
using Microsoft.AppCenter.Crashes;
#endif
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ApiService
	{

		private readonly PlatformService Platform;
		private readonly SettingsService Settings;

		public ServerInfo ServerInfo = null!;
		public ControlFlags ControlFlags = new ControlFlags();

		public RestClient Client { get; private set; } = null!;

		public ApiService(PlatformService platform, SettingsService settings)
		{
			Platform = platform;
			Settings = settings;
		}

		public bool RefreshSettings(ServerProfile profile)
		{
			if (!Uri.IsWellFormedUriString(profile.ServerAddress, UriKind.Absolute))
				return false;
			var options = new RestClientOptions(profile.ServerAddress) { UserAgent = "LRReader" };
			Client = new RestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
			if (!string.IsNullOrEmpty(profile.ServerApiKey))
			{
				var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(profile.ServerApiKey));
				Client.AddDefaultHeader("Authorization", $"Bearer {base64Key}");
			}
			return true;
		}

		public Task<bool> Validate()
		{
#if false
#if WINDOWS_UWP
			await Crashes.SetEnabledAsync(false); // Disable to prevent false-positive errors
#endif
			var archives = await ArchivesProvider.Validate();
			var categories = await CategoriesProvider.Validate();
			var database = await DatabaseProvider.Validate();

			if (archives && categories && database)
			{
#if WINDOWS_UWP
				await Crashes.SetEnabledAsync(Settings.CrashReporting);
#endif
				Settings.Profile.AcceptedDisclaimer = false;
				Settings.SaveProfiles();
				return true;
			}
			if (Settings.Profile.AcceptedDisclaimer)
			{
#if WINDOWS_UWP
				await Crashes.SetEnabledAsync(false);
#endif
				return true;
			}

			var result = (await Platform.OpenDialog(Dialog.ValidateApi, archives, categories, database)) == IDialogResult.Primary;

			Settings.Profile.AcceptedDisclaimer = result;
			if (result)
			{
#if WINDOWS_UWP
				await Crashes.SetEnabledAsync(false);
#endif
			}
			Settings.SaveProfiles();
			return result;
#else
			return Task.FromResult(true);
#endif
		}
	}

	public class ControlFlags
	{
		public bool ProgressTracking = false;

		public bool BrokenCache;

		public void Check(ServerInfo serverInfo)
		{
			BrokenCache = true;

			ProgressTracking = serverInfo.server_tracks_progress;
		}
	}
}
