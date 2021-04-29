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

		public Task RunAsync(Action action) => Dispatcher.InvokeAsync(action);
	}
}
