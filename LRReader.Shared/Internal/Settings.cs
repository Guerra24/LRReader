using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Internal
{
	public interface ISettingsStorage
	{
		void StoreObjectLocal(string key, object obj);

		void StoreObjectRoamed(string key, object obj);

		T GetObjectLocal<T>(string key);

		T GetObjectLocal<T>(string key, T def);

		T GetObjectRoamed<T>(string key);

		T GetObjectRoamed<T>(string key, T def);
	}
}
