using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LRReader.UWP.Servicing;

public static class ProcessUtil
{
	public static async Task<int> LaunchAdmin(string exe, string command = "")
	{
		using var process = new Process();
		process.StartInfo.UseShellExecute = true;
		process.StartInfo.Verb = "runas";
		process.StartInfo.FileName = exe;
		process.StartInfo.Arguments = command;
		try
		{
			process.Start();
			var result = await process.WaitForExitStatusAsync().ConfigureAwait(false);
			return result.ExitCode;
		}
		catch (Win32Exception) { }
		return -99;
	}
}
