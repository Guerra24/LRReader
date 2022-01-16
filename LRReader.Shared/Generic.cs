using System.Threading.Tasks;

namespace LRReader.Shared
{
	public delegate Task AsyncAction<T>(T obj);

	public static class AsyncActionExtensions
	{
		public static Task InvokeAsync<T>(this AsyncAction<T>? action, T obj)
		{
			return action?.Invoke(obj) ?? Task.FromResult(0);
		}
	}
}
