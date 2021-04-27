using System;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public interface IDispatcherService
	{
		void Init();

		Task RunAsync(Action action);
	}

	public class StubDispatcherService : IDispatcherService
	{
		public void Init()
		{
		}

		public Task RunAsync(Action action)
		{
			action.Invoke();
			return null;
		}
	}
}
