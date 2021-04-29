namespace LRReader.Shared.Extensions
{
	public static class StringExtensions
	{
		public static string AsFormat(this string format, params object[] args) => string.Format(format, args);
	}
}
