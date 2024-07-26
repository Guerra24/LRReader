using System;
using System.Globalization;
using System.Threading.Tasks;
using LRReader.Shared.Models;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace LRReader.Shared.Services
{
	public abstract class UpdatesService
	{
		protected readonly PlatformService Platform;
		protected readonly ISettingsStorageService SettingsStorage;
		protected readonly SettingsService Settings;

		public Version MIN_VERSION = new Version(0, 9, 10);
		public Version MAX_VERSION = new Version(0, 9, 21);

		protected readonly RestClient client;

		public UpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings)
		{
			Platform = platform;
			SettingsStorage = settingsStorage;
			Settings = settings;
#if DEBUG
			var uri = new Uri("http://localhost:5000/");
#else
			var uri = new Uri("https://api.guerra24.net/");
#endif
			var options = new RestClientOptions(uri) { UserAgent = "LRReader" };
			client = new RestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
		}

		public abstract Task<CheckForUpdatesResult> CheckForUpdates();

		public abstract Task<UpdateResult> DownloadAndInstall(IProgress<double> progress, CheckForUpdatesResult? check = null);

		public abstract bool CanAutoUpdate();

		public async Task<UpdateChangelog> GetChangelog(Version version)
		{
			var rq = new RestRequest("lrr/upgrade/changelog");
			rq.AddParameter("version", version.ToString());
			rq.AddParameter("lang", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

			var r = await client.ExecuteGetAsync(rq);

			var result = await r.GetResultInternal<UpdateChangelog>();

			if (!string.IsNullOrEmpty(r.ErrorMessage))
				return new UpdateChangelog { Name = "", Content = "" };
			if (result.OK)
				return result.Data!;
			return new UpdateChangelog { Name = "", Content = "" };
		}

		public async Task UpdateSupportedRange()
		{
			var rq = new RestRequest("lrr/compat");
			rq.AddParameter("version", Platform.Version.ToString());

			var r = await client.ExecuteGetAsync(rq);

			var result = await r.GetResultInternal<VersionSupportedRange>();

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				ReadVersion();
				return;
			}
			if (result.OK && result.Data != null)
			{
				var range = result.Data;
				MIN_VERSION = range.minSupported;
				MAX_VERSION = range.maxSupported;
				SettingsStorage.StoreObjectLocal(MIN_VERSION.ToString(), "MinVersion");
				SettingsStorage.StoreObjectLocal(MAX_VERSION.ToString(), "MaxVersion");
			}
			else
			{
				ReadVersion();
			}
		}

		private void ReadVersion()
		{
			MIN_VERSION = Version.Parse(SettingsStorage.GetObjectLocal(MIN_VERSION.ToString(), "MinVersion"));
			MAX_VERSION = Version.Parse(SettingsStorage.GetObjectLocal(MAX_VERSION.ToString(), "MaxVersion"));
		}

	}

	public class StubUpdatesService : UpdatesService
	{
		public StubUpdatesService(PlatformService platform, ISettingsStorageService settingsStorage, SettingsService settings) : base(platform, settingsStorage, settings) { }

		public override bool CanAutoUpdate() => false;

		public override Task<CheckForUpdatesResult> CheckForUpdates() => Task.Run(() => new CheckForUpdatesResult { Result = false });

		public override Task<UpdateResult> DownloadAndInstall(IProgress<double> progress, CheckForUpdatesResult? check = null) => Task.FromResult(new UpdateResult { Result = false, ErrorCode = -1, ErrorMessage = "Stub" });
	}
}
