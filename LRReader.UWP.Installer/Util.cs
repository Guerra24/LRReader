using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.UWP.Installer
{
	public static class Util
	{
		public static Task StartAndWaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
		{
			var tcs = new TaskCompletionSource<object>();
			process.EnableRaisingEvents = true;
			process.Exited += (sender, args) => tcs.TrySetResult(null);
			if (cancellationToken != default)
				cancellationToken.Register(() => tcs.SetCanceled());
			process.Start();
			return process.HasExited ? Task.CompletedTask : tcs.Task;
		}
	}
}
