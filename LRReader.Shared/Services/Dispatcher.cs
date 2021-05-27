using System;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{
	public interface IDispatcherService
	{
		void Init();

		Task RunAsync(Action action);

		bool Run(Action action);
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

		public bool Run(Action action)
		{
			action.Invoke();
			return true;
		}
	}
}
