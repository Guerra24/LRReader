using Avalonia.Threading;
using LRReader.Shared.Services;
using System;
using System.Threading.Tasks;

namespace LRReader.Avalonia.Services
{

	public class DispatcherService : IDispatcherService
	{
		private Dispatcher Dispatcher;

		public void Init() => Dispatcher = Dispatcher.UIThread;

		public bool Run(Action action, int priority = 0)
		{
			action.Invoke();
			return true;
		}

		public Task RunAsync(Action action) => Dispatcher.InvokeAsync(action);

		public Task RunAsync(Action action, int priority = 0) => Dispatcher.InvokeAsync(action);
	}
}
