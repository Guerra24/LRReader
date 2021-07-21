using System;
using System.Collections.Generic;
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
