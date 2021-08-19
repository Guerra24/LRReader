using System.Threading.Tasks;

namespace LRReader.Shared.Services
{

	//[CallerMemberName]
	public interface ISettingsStorageService : IService
	{
		void StoreObjectLocal(string key, object obj);

		void StoreObjectRoamed(string key, object obj);

		T GetObjectLocal<T>(string key);

		T GetObjectLocal<T>(string key, T def);

		T GetObjectRoamed<T>(string key);

		T GetObjectRoamed<T>(string key, T def);

		void DeleteObjectLocal(string key);

		void DeleteObjectRoamed(string key);
	}

	public class StubSettingsStorageService : ISettingsStorageService
	{
		public Task Init() => Task.Delay(1);

		public T GetObjectLocal<T>(string key) => GetObjectLocal<T>(key, default);

		public T GetObjectLocal<T>(string key, T def) => def;

		public T GetObjectRoamed<T>(string key) => GetObjectRoamed<T>(key, default);

		public T GetObjectRoamed<T>(string key, T def) => def;

		public void StoreObjectLocal(string key, object obj)
		{
		}

		public void StoreObjectRoamed(string key, object obj)
		{
		}

		public void DeleteObjectLocal(string key)
		{
		}

		public void DeleteObjectRoamed(string key)
		{
		}
	}
}
