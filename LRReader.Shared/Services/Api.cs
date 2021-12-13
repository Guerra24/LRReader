using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using Microsoft.AppCenter.Crashes;
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
			Client = new RestClient();
			Client.UseNewtonsoftJson();
			Client.BaseUrl = new Uri(profile.ServerAddress);
			Client.UserAgent = "LRReader";
			if (!string.IsNullOrEmpty(profile.ServerApiKey))
			{
				var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(profile.ServerApiKey));
				Client.AddDefaultHeader("Authorization", $"Bearer {base64Key}");
			}
			return true;
		}

		public async Task<bool> Validate()
		{
			await Crashes.SetEnabledAsync(false); // Disable to prevent false-positive errors

			var archives = await ArchivesProvider.Validate();
			var categories = await CategoriesProvider.Validate();
			var database = await DatabaseProvider.Validate();

			if (archives && categories && database)
			{
				await Crashes.SetEnabledAsync(Settings.CrashReporting);
				Settings.Profile.AcceptedDisclaimer = false;
				Settings.SaveProfiles();
				return true;
			}
			if (Settings.Profile.AcceptedDisclaimer)
			{
				await Crashes.SetEnabledAsync(false);
				return true;
			}

			var result = (await Platform.OpenDialog(Dialog.ValidateApi, archives, categories, database)) == IDialogResult.Primary;

			Settings.Profile.AcceptedDisclaimer = result;
			if (result)
				await Crashes.SetEnabledAsync(false);
			Settings.SaveProfiles();
			return result;
		}

	}

	public class ControlFlags
	{
		public bool ProgressTracking = false;

		public bool V077 = false;
		public bool V078 = false;
		public bool V079 = false;
		public bool V082 = false;

		public bool V078Edit => V078 & Service.Settings.Profile.HasApiKey;

		public void Check(ServerInfo serverInfo)
		{
			V077 = serverInfo.version >= new Version(0, 7, 7);
			V078 = serverInfo.version >= new Version(0, 7, 8);
			V079 = serverInfo.version >= new Version(0, 7, 9);
			V082 = serverInfo.version >= new Version(0, 8, 2);

			ProgressTracking = (!V079 && V077) || serverInfo.server_tracks_progress;
		}
	}
}
