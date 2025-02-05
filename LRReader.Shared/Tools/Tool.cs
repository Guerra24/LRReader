﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LRReader.Shared.Services;
using Sentry;

namespace LRReader.Shared.Tools
{

	public struct ToolProgress<T>
	{
		public T Status { get; set; }
		public int MaxProgress { get; set; }
		public int CurrentProgress { get; set; }
		public int MaxSteps { get; set; }
		public int CurrentStep { get; set; }
		public long Time { get; set; }

		public ToolProgress(T status, int maxProgress = -1, int currentProgress = -1, int maxSteps = -1, int currentStep = -1, long time = -1)
		{
			Status = status;
			MaxProgress = maxProgress;
			CurrentProgress = currentProgress;
			MaxSteps = maxSteps;
			CurrentStep = currentStep;
			Time = time;
		}

		public override string ToString()
		{
			return $"{Status}-{MaxProgress}-{CurrentProgress}-{MaxSteps}-{CurrentStep}-{Time}";
		}
	}

	public struct ToolResult<T, E>
	{
		public bool Ok { get; set; }
		public T Data { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public E? Error { get; set; }
	}

	public interface IToolParams
	{

	}

	public abstract class Tool<T, P, R, E> where P : IToolParams
	{

		private readonly PlatformService Platform;

		private IProgress<ToolProgress<T>>? Progress;

		public Tool(PlatformService platform)
		{
			Platform = platform;
		}

		public async Task<ToolResult<R, E>> Execute(P @params, int threads, IProgress<ToolProgress<T>>? progress = null)
		{
			Progress = progress;
			var result = new ToolResult<R, E> { Title = Platform.GetLocalizedString("Tools/GenericTool/Error") };
			try
			{
				result = await Process(@params, threads);
			}
			catch (Exception e)
			{
				SentrySdk.CaptureException(e);
			}
			GC.Collect();
			return result;
		}

		protected abstract Task<ToolResult<R, E>> Process(P @params, int threads);

		protected ToolResult<R, E> EarlyExit(string title, string description, E? error = default) => new ToolResult<R, E> { Title = title, Description = description, Error = error };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void UpdateProgress(T status, int maxProgress = -1, int currentProgress = -1, int maxSteps = -1, int currentStep = -1, long time = -1)
		{
			Progress?.Report(new ToolProgress<T>(status, maxProgress, currentProgress, maxSteps, currentStep, time));
		}
	}

	public static class Util
	{
		public static async Task WhenAllEx(this IList<Task> tasks, Action<IList<Task>> reportProgressAction)
		{
			var whenAllTask = Task.WhenAll(tasks);
			while (true)
			{
				var timer = Task.Delay(50);
				await Task.WhenAny(whenAllTask, timer);
				if (whenAllTask.IsCompleted)
					return;
				reportProgressAction(tasks);
			}
		}
	}
}
