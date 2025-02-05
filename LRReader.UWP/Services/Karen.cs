using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace LRReader.UWP.Services
{
	public class KarenService : IKarenService
	{
		private BackgroundTaskDeferral? Deferral;
		private AppServiceConnection? Connection;


		public void Connect(object args)
		{
			var instance = (IBackgroundTaskInstance)args;
			var details = (AppServiceTriggerDetails)instance.TriggerDetails;
			instance.Canceled += Instance_Canceled;
			Deferral = instance.GetDeferral();
			Connection = details.AppServiceConnection;
		}

		public async Task<IDictionary<string, object>?> SendMessage(IDictionary<string, object> data)
		{
			if (Connection == null)
				return null;
			var set = new ValueSet();
			foreach (var keyPair in data)
				set.Add(keyPair);

			var res = await Connection?.SendMessageAsync(set);

			if (res.Status == AppServiceResponseStatus.Success)
				return res.Message;
			return null;
		}

		public async Task<T?> LoadSetting<T>(SettingType type)
		{
			if (Connection == null)
				return default;
			var data = new Dictionary<string, object>();
			data["PacketType"] = (int)PacketType.InstanceSetting;
			data["PacketSettingOperation"] = (int)SettingOperation.Load;
			data["PacketSettingType"] = (int)type;
			var result = await SendMessage(data);
			if (result != null && result.TryGetValue("PacketValue", out var res))
				return (T)res;
			return default;
		}

		public async Task SaveSetting(SettingType type, object value)
		{
			if (Connection == null)
				return;
			var data = new Dictionary<string, object>();
			data["PacketType"] = (int)PacketType.InstanceSetting;
			data["PacketSettingOperation"] = (int)SettingOperation.Save;
			data["PacketSettingType"] = (int)type;
			data["PacketValue"] = value;
			await SendMessage(data);
			return;
		}

		public void Disconnect()
		{
			Deferral?.Complete();
			Connection?.Dispose();
			Connection = null;
			Deferral = null;
		}

		public bool IsConnected => Connection != null;

		private void Instance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
		{
			Disconnect();
		}
	}
}
