using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.UWP.Servicing;

public static class ProcessExtension
{
	public static Task StartAndWaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
	{
		var tcs = new TaskCompletionSource<object?>();
		process.EnableRaisingEvents = true;
		process.Exited += (sender, args) => tcs.TrySetResult(null);
		if (cancellationToken != default)
			cancellationToken.Register(tcs.SetCanceled);
		process.Start();
		return process.HasExited ? Task.CompletedTask : tcs.Task;
	}
}

public static class ProcessUtil
{
	public static async Task<int> LaunchAdmin(string exe, string command = "")
	{
		using var process = new Process();
		process.StartInfo.Verb = "runas";
		process.StartInfo.FileName = exe;
		process.StartInfo.Arguments = command;
		try
		{
			await process.StartAndWaitForExitAsync().ConfigureAwait(false);
			return process.ExitCode;
		}
		catch (Win32Exception) { }
		return -99;
	}
}
