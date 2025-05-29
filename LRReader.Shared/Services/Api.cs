using LRReader.Shared.Models.Main;
#if false
using Microsoft.AppCenter.Crashes;
#endif
using RestSharp;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public class ApiService
	{

		public ServerInfo ServerInfo = null!;
		public ControlFlags ControlFlags = new ControlFlags();

		public RestClient Client { get; private set; } = null!;

		public bool RefreshSettings(ServerProfile profile, string lang)
		{
			if (!Uri.IsWellFormedUriString(profile.ServerAddress, UriKind.Absolute))
				return false;
			var options = new RestClientOptions(profile.ServerAddress)
			{
				UserAgent = "LRReader",
				ConfigureMessageHandler = (handler) =>
				{
					// Restsharp uses HttpClientHandler by default but we do not want that so kill it
					handler.Dispose();
					var socket = new SocketsHttpHandler();
					socket.MaxConnectionsPerServer = 20; // Limit this so that mojo does not think we are dos'ing it
					socket.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30); // Mojo default idle timeout
					socket.UseCookies = false;
					socket.AutomaticDecompression = System.Net.DecompressionMethods.All;
					return socket;
				}
			};
			Client?.Dispose();
			Client = new RestClient(options);
			if (!string.IsNullOrEmpty(profile.ServerApiKey))
			{
				var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(profile.ServerApiKey));
				Client.AddDefaultHeader("Authorization", $"Bearer {base64Key}");
				Client.AddDefaultHeader("Accept-Language", lang);
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

		public bool V0940;

		public bool BrokenCache;

		public bool V0940Edit => V0940 & Service.Settings.Profile.HasApiKey;

		public void Check(ServerInfo serverInfo)
		{
			BrokenCache = true;

			V0940 = serverInfo.version >= new Version(0, 9, 40);

			ProgressTracking = serverInfo.server_tracks_progress;
		}
	}
}
