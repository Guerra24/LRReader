using LRReader.Shared.Models.Main;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Text;

namespace LRReader.Shared.Services
{
	public class ApiService
	{
		public ServerInfo ServerInfo;
		public ControlFlags ControlFlags = new ControlFlags();

		private RestClient client;

		public void RefreshSettings(ServerProfile profile)
		{
			client = new RestClient();
			client.UseNewtonsoftJson();
			client.BaseUrl = new Uri(profile.ServerAddress);
			client.UserAgent = "LRReader";
			if (!string.IsNullOrEmpty(profile.ServerApiKey))
			{
				var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(profile.ServerApiKey));
				client.AddDefaultHeader("Authorization", $"Bearer {base64Key}");
			}
		}

		public RestClient GetClient()
		{
			return client;
		}
	}

	public class ControlFlags
	{
		public bool CategoriesEnabled = true;
		public bool V077 = false;
		public bool V078 = false;

		public bool V078Edit => V078 & Service.Settings.Profile.HasApiKey;
		public bool CategoriesEnabledEdit => CategoriesEnabled & Service.Settings.Profile.HasApiKey;

		public void Check(ServerInfo serverInfo)
		{
			if (serverInfo.version == new Version(0, 7, 5))
				CategoriesEnabled = false;
			V077 = serverInfo.version >= new Version(0, 7, 7);
			V078 = serverInfo.version >= new Version(0, 7, 8);
		}
	}
}
