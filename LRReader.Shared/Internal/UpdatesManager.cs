using LRReader.Shared.Models;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.Shared.Internal
{
	public class UpdatesManager
	{
		public static Version MIN_VERSION = new Version(0, 7, 3);
		public static Version MAX_VERSION = new Version(0, 7, 8);

		private RestClient client;

		public UpdatesManager()
		{
			client = new RestClient();
			client.UseNewtonsoftJson();
			client.BaseUrl = new Uri("https://api.guerra24.net/");
			client.UserAgent = "LRReader";
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public async Task<ReleaseInfo> CheckUpdates(Version current)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
#if SIDELOAD
			var rq = new RestRequest("lrr/upgrade/latest");

			var r = await client.ExecuteGetAsync(rq);

			var result = await r.GetResultInternal<ReleaseInfo>();

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				return null;
			}
			if (result.OK)
			{
				var info = result.Data;
				if (info.version > current)
				{
					info.link = $"ms-appinstaller:?source=https://s3.guerra24.net/projects/lrr/{info.version}/LRReader.UWP.appinstaller";
					return info;
				}
			}
#endif
			return null;
		}

		public async Task<string> GetChangelog(Version current)
		{
			var rq = new RestRequest("lrr/upgrade/latest");

			var r = await client.ExecuteGetAsync(rq);

			var result = await r.GetResultInternal<ReleaseInfo>();

			if (!string.IsNullOrEmpty(r.ErrorMessage))
			{
				return null;
			}
			if (result.OK)
			{
				var info = result.Data;
				if (info.version == current)
					return result.Data.body;
			}
			return null;
		}

		public async Task UpdateSupportedRange(Version current)
		{
			var rq = new RestRequest("lrr/compat");
			rq.AddParameter("version", current.ToString());

			var r = await client.ExecuteGetAsync(rq);

			var result = await r.GetResultInternal<VersionSupportedRange>();

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
				SettingsStorage.StoreObjectLocal("MinVersion", MIN_VERSION.ToString());
				SettingsStorage.StoreObjectLocal("MaxVersion", MAX_VERSION.ToString());
			}
			else
			{
				ReadVersion();
			}
		}

		private void ReadVersion()
		{
			MIN_VERSION = Version.Parse(SettingsStorage.GetObjectLocal("MinVersion", MIN_VERSION.ToString()));
			MAX_VERSION = Version.Parse(SettingsStorage.GetObjectLocal("MaxVersion", MAX_VERSION.ToString()));
		}
	}

	public class ReleaseInfo
	{
		public string name { get; set; }
		public string body { get; set; }
		[JsonConverter(typeof(VersionConverter))]
		public Version version { get; set; }
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
