using LRReader.Shared.Services;
using Microsoft.Toolkit.Uwp;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace LRReader.UWP.Services
{
	public class DispatcherService : IDispatcherService
	{
		private DispatcherQueue Dispatcher;

		public void Init() => Dispatcher = DispatcherQueue.GetForCurrentThread();

		public Task RunAsync(Action action, int priority) => Dispatcher.EnqueueAsync(action, (DispatcherQueuePriority)priority);

		public bool Run(Action action, int priority) => Dispatcher.TryEnqueue((DispatcherQueuePriority)priority, () => action.Invoke());

	}

}
