using LRReader.Shared.Models;
using LRReader.Shared.Models.Api;
using Newtonsoft.Json;
using Octokit;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Threading.Tasks;

namespace LRReader.Shared.Internal
{
	public class UpdatesManager
	{
		public static Version MIN_VERSION = new Version(0, 7, 1);
		public static Version MAX_VERSION = new Version(0, 7, 5);

		private GitHubClient githubClient;
		private RestClient client;

		public UpdatesManager()
		{
			githubClient = new GitHubClient(new ProductHeaderValue("LRReader"));
			client = new RestClient();
			client.UseNewtonsoftJson();
			client.BaseUrl = new Uri("https://api.guerra24.net/");
			client.UserAgent = "LRReader";
		}

		public async Task<ReleaseInfo> CheckUpdates(Version current)
		{
			try
			{
				var release = await githubClient.Repository.Release.GetLatest(201592446);
				if (!release.TagName.StartsWith("v"))
					return null;
				var newer = new Version(release.TagName.Substring(1));
				if (newer > current)
				{
					var info = new ReleaseInfo();
					info.name = release.Name;
					info.body = release.Body;
					info.version = newer.ToString();
					info.link = $"ms-appinstaller:?source=https://s3.guerra24.net/projects/lrr/{info.version}/LRReader.UWP.appinstaller";
					return info;
				}
			}
			catch (Exception)
			{
			}
			return null;
		}

		public async Task UpdateSupportedRange(Version current)
		{
			var rq = new RestRequest("lrr/compat");
			rq.AddParameter("version", current.ToString());

			var r = await client.ExecuteGetAsync(rq);

			var result = await LRRApi.GetResult<VersionSupportedRange>(r);

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				ReadVersion();
				return;
			}
			if (result.OK)
			{
				var range = result.Data;
				MIN_VERSION = range.minSupported;
				MAX_VERSION = range.maxSupported;
				SharedGlobal.SettingsStorage.StoreObjectLocal("MinVersion", MIN_VERSION.ToString());
				SharedGlobal.SettingsStorage.StoreObjectLocal("MaxVersion", MAX_VERSION.ToString());
			}
			else
			{
				ReadVersion();
			}
		}

		private void ReadVersion()
		{
			MIN_VERSION = Version.Parse(SharedGlobal.SettingsStorage.GetObjectLocal("MinVersion", MIN_VERSION.ToString()));
			MAX_VERSION = Version.Parse(SharedGlobal.SettingsStorage.GetObjectLocal("MaxVersion", MAX_VERSION.ToString()));
		}
	}

	public class ReleaseInfo
	{
		public string name { get; set; }
		public string body { get; set; }
		public string version { get; set; }
		public string link { get; set; }
	}

	public class VersionSupportedRange
	{
		[JsonConverter(typeof(VersionConverter))]
		public Version minSupported { get; set; }
		[JsonConverter(typeof(VersionConverter))]
		public Version maxSupported { get; set; }
	}
}
