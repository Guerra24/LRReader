﻿using LRReader.Shared.Services;
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

		public Task RunAsync(Action action) => Dispatcher.EnqueueAsync(action);

		public bool Run(Action action) => Dispatcher.TryEnqueue(() => action.Invoke());

	}

}
