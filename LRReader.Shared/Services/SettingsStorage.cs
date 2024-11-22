using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{

	//[CallerMemberName]
	public interface ISettingsStorageService : IService
	{
		T? GetObjectLocal<T>([CallerMemberName] string? key = null);

		T? GetObjectRoamed<T>([CallerMemberName] string? key = null);

		void DeleteObjectLocal(string key);

		void DeleteObjectRoamed(string key);

		void StoreObjectLocal(object obj, [CallerMemberName] string? key = null);

		void StoreObjectRoamed(object obj, [CallerMemberName] string? key = null);

		[return: NotNullIfNotNull("def")]
		T? GetObjectLocal<T>(T? def, [CallerMemberName] string? key = null);

		[return: NotNullIfNotNull("def")]
		T? GetObjectRoamed<T>(T? def, [CallerMemberName] string? key = null);

		bool ExistLocal(string key);
		bool ExistRoamed(string key);

	}

	public class StubSettingsStorageService : ISettingsStorageService
	{
		public Task Init() => Task.CompletedTask;

		public T? GetObjectLocal<T>([CallerMemberName] string? key = null) => GetObjectLocal<T>(default, key);

		public T? GetObjectRoamed<T>([CallerMemberName] string? key = null) => GetObjectRoamed<T>(default, key);

		public void DeleteObjectLocal(string key)
		{
		}

		public void DeleteObjectRoamed(string key)
		{
		}

		public void StoreObjectLocal(object obj, [CallerMemberName] string? key = null)
		{
		}

		public void StoreObjectRoamed(object obj, [CallerMemberName] string? key = null)
		{
		}

		[return: NotNullIfNotNull("def")]
		public T? GetObjectLocal<T>(T? def, [CallerMemberName] string? key = null) => def;

		[return: NotNullIfNotNull("def")]
		public T? GetObjectRoamed<T>(T? def, [CallerMemberName] string? key = null) => def;

		public bool ExistLocal(string key) => false;

		public bool ExistRoamed(string key) => false;
	}
}
