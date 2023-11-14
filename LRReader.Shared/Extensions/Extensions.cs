using System;

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
