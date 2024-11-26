using System;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public interface IDispatcherService
	{
		void Init();

		Task RunAsync(Action action, int priority = 0);

		bool Run(Action action, int priority = 0);
	}

	public class StubDispatcherService : IDispatcherService
	{
		public void Init()
		{
		}

		public Task RunAsync(Action action, int priority)
		{
			action.Invoke();
			return Task.CompletedTask;
		}

		public bool Run(Action action, int priority)
		{
			action.Invoke();
			return true;
		}
	}
}
