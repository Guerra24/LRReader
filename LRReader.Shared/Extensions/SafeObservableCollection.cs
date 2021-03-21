using System.Collections.ObjectModel;
using System.Linq;

namespace LRReader.Shared.Extensions
{
	public static class SafeObservableCollection
	{
		public static void SafeClear<T>(this ObservableCollection<T> collection)
		{
			while (collection.Any())
				collection.RemoveAt(0);
		}
	}
}
