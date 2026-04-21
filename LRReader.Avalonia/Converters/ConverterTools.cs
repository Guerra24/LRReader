// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace LRReader.Avalonia.Converters;

internal static class ConverterTools
{

	internal static bool TryParseBool(object? parameter)
	{
		var parsed = false;
		if (parameter != null)
		{
			bool.TryParse(parameter.ToString(), out parsed);
		}

		return parsed;
	}

	internal static object Convert(object value, Type targetType)
	{
		//if (targetType.IsInstanceOfType(value))
		{
			return value;
		}
		/*else
		{
			return XamlBindingHelper.ConvertValue(targetType, value);
		}*/
	}
}
