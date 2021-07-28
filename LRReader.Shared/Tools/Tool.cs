using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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

	public struct ToolResult<T>
	{
		public bool Ok { get; set; }
		public T Data { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}

	public interface IToolParams
	{

	}

	public abstract class Tool<T, P, R> where P : IToolParams
	{

		private Subject<ToolProgress<T>> progressFilter;

		protected volatile bool earlyExit;

		public async Task<ToolResult<R>> Execute(P @params, int threads, IProgress<ToolProgress<T>> progress = null)
		{
			progressFilter = new Subject<ToolProgress<T>>();
			progressFilter.Window(TimeSpan.FromMilliseconds(1000)).SelectMany(i => i.TakeLast(1)).Subscribe(p => progress?.Report(p));
			ToolResult<R> result = new ToolResult<R> { Title = "An unknown error occurred" };
			try
			{
				result = await Process(@params, threads);
			}
			catch (Exception e)
			{
				Crashes.TrackError(e);
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			progressFilter.OnCompleted();
			return result;
		}

		protected abstract Task<ToolResult<R>> Process(P @params, int threads);

		protected ToolResult<R> EarlyExit(string title, string description) => new ToolResult<R> { Title = title, Description = description };

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void UpdateProgress(T status, int maxProgress = -1, int currentProgress = -1, int maxSteps = -1, int currentStep = -1, long time = -1) => progressFilter.OnNext(new ToolProgress<T>(status, maxProgress, currentProgress, maxSteps, currentStep, time));
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
