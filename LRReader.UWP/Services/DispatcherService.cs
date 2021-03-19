using Microsoft.Toolkit.Uwp;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace LRReader.UWP.Services
{
	public static class DispatcherService
	{
		public static DispatcherQueue Dispatcher;

		public static void Init() => Dispatcher = DispatcherQueue.GetForCurrentThread();

		public static Task RunAsync(Action action) => Dispatcher.EnqueueAsync(action);

	}
}
