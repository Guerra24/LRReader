using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using LRReader.Shared.Services;
using Windows.System;

namespace LRReader.UWP.Services
{
	public class DispatcherService : IDispatcherService
	{
		private DispatcherQueue Dispatcher = null!;

		public void Init() => Dispatcher = DispatcherQueue.GetForCurrentThread();

		public Task RunAsync(Action action, int priority) => Dispatcher.EnqueueAsync(action, (DispatcherQueuePriority)priority);

		public bool Run(Action action, int priority) => Dispatcher.TryEnqueue((DispatcherQueuePriority)priority, () => action.Invoke());

	}

}
