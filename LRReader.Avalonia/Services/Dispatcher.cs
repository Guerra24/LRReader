using Avalonia.Threading;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Services
{

	public class DispatcherService : IDispatcherService
	{
		private Dispatcher Dispatcher = null!;

		public void Init() => Dispatcher = Dispatcher.UIThread;

		public bool Run(Action action, int priority = 0)
		{
			Dispatcher.Post(action, priority);
			return true;
		}

		public Task RunAsync(Action action, int priority = 0) => Dispatcher.InvokeAsync(action).GetTask();
	}
}
