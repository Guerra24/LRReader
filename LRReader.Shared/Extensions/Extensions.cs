using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LRReader.Shared.Extensions;

public static class StringExtensions
{
	public static string AsFormat(this string format, params object[] args) => string.Format(format, args);

	public static string UpperFirstLetter(this string str)
	{
		if (str.Length == 0)
			return "";
		else if (str.Length == 1)
			return char.ToUpper(str[0]).ToString();
		else
			return char.ToUpper(str[0]) + str.Substring(1);
	}
}

public static class CompareExtensions
{
	public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
	{
		if (val.CompareTo(min) < 0) return min;
		else if (val.CompareTo(max) > 0) return max;
		else return val;
	}
}

public class GridViewExtParameter
{
	public bool Ctrl { get; }
	public object Item { get; }

	public GridViewExtParameter(bool ctrl, object item)
	{
		Ctrl = ctrl;
		Item = item;
	}
}

public static class MinionExtensions
{

	public static async Task<bool> WaitForMinionJob<T>(this T minionJob, CancellationToken cancellationToken = default) where T : MinionJob
	{
		while (true)
		{
			if (cancellationToken.IsCancellationRequested)
				return false;
			var job = await ServerProvider.GetMinionStatus(minionJob.job).ConfigureAwait(false);
			if (job == null || job.state == null)
				return false;
			if (job.state.Equals("finished"))
				return true;
			if (job.state.Equals("failed"))
				return false;
			await Task.Delay(100).ConfigureAwait(false);
		}
	}
}
