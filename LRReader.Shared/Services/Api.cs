using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
#if WINDOWS_UWP
using Microsoft.AppCenter.Crashes;
#endif
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ApiService
	{

		private readonly PlatformService Platform;
		private readonly SettingsService Settings;

		[AllowNull]
		public ServerInfo ServerInfo;
		public ControlFlags ControlFlags = new ControlFlags();

		[AllowNull]
		public RestClient Client { get; private set; }

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
			Client = new RestClient(options);
			Client.UseNewtonsoftJson();
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
			return Task.Run(() => true);
#endif
		}
	}

	public class ControlFlags
	{
		public bool ProgressTracking = false;

		public bool V077;
		public bool V078;
		public bool V079;
		public bool V082;
		public bool V084;

		public bool V078Edit => V078 & Service.Settings.Profile.HasApiKey;

		public void Check(ServerInfo serverInfo)
		{
			V077 = serverInfo.version >= new Version(0, 7, 7);
			V078 = serverInfo.version >= new Version(0, 7, 8);
			V079 = serverInfo.version >= new Version(0, 7, 9);
			V082 = serverInfo.version >= new Version(0, 8, 2);
			V084 = serverInfo.version >= new Version(0, 8, 4);

			ProgressTracking = (!V079 && V077) || serverInfo.server_tracks_progress;
		}
	}
}
