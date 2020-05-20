using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Internal
{
	public class UpdatesManager
	{
		private GitHubClient client;

		public UpdatesManager()
		{
			client = new GitHubClient(new ProductHeaderValue("LRReader"));
		}

		public async Task<ReleaseInfo> CheckUpdates(Version current)
		{
			var release = await client.Repository.Release.GetLatest(201592446);
			if (!release.TagName.StartsWith("v"))
				return null;
			var newer = new Version(release.TagName.Substring(1));
			if (newer > current)
			{
				var info = new ReleaseInfo();
				info.name = release.Name;
				info.body = release.Body;
				info.version = newer.ToString();
				return info;
			}
			return null;
		}
	}

	public class ReleaseInfo
	{
		public string name { get; set; }
		public string body { get; set; }
		public string version { get; set; }
	}
}
