using System;
using System.Collections.Generic;
using System.Text;

namespace LRReader.Shared.Internal
{
	internal static class Util
	{
		public static DateTime UnixTimeToDateTime(long unixtime)
		{
			DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
			return dtDateTime;
		}

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
}
