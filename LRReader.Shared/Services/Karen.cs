using System.Collections.Generic;
using System.Threading.Tasks;
using LRReader.Shared.Models;

namespace LRReader.Shared.Services
{
	public interface IKarenService
	{
		public void Connect(object instance);

		public Task<IDictionary<string, object>?> SendMessage(IDictionary<string, object> data);

		public Task<T?> LoadSetting<T>(SettingType type);

		public Task SaveSetting(SettingType type, object value);

		public void Disconnect();

		public bool IsConnected { get; }
	}

	public class StubKarenService : IKarenService
	{
		public void Connect(object instance) { }

		public Task<IDictionary<string, object>?> SendMessage(IDictionary<string, object> data) => Task.FromResult<IDictionary<string, object>?>(null);

		public Task<T?> LoadSetting<T>(SettingType type) => Task.FromResult<T?>(default);

		public Task SaveSetting(SettingType type, object value) => Task.CompletedTask;

		public void Disconnect() { }

		public bool IsConnected => false;
	}
}
