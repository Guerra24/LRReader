// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//https://github.com/CommunityToolkit/Windows/tree/main/components/Converters/src

using Avalonia.Data.Converters;
using System.Collections;
using System.Globalization;

namespace LRReader.Avalonia.Converters
{

	public class DoubleToObjectConverter : IValueConverter
	{
		public object? FalseValue { get; set; }

		public object? TrueValue { get; set; }

		public object? NullValue { get; set; }

		public double GreaterThan { get; set; } = double.NaN;

		public double LessThan { get; set; } = double.NaN;

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return NullValue;
			}

			double vd = 0.0; // DEFAULT?
			if (value is double dbl)
			{
				vd = dbl;
			}
			else if (double.TryParse(value.ToString(), out double result))
			{
				vd = result;
			}

			var boolValue = false;

			if (!double.IsNaN(GreaterThan) && !double.IsNaN(LessThan))
			{
				if (vd > GreaterThan && vd < LessThan)
				{
					boolValue = true;
				}
			}
			else if (!double.IsNaN(GreaterThan) && vd > GreaterThan)
			{
				boolValue = true;
			}
			else if (!double.IsNaN(LessThan) && vd < LessThan)
			{
				boolValue = true;
			}

			// Negate if needed
			if (ConverterTools.TryParseBool(parameter))
			{
				boolValue = !boolValue;
			}

			return ConverterTools.Convert(boolValue ? TrueValue : FalseValue, targetType);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

	}

	public partial class BoolToObjectConverter : IValueConverter
	{

		public object? FalseValue { get; set; }

		public object? TrueValue { get; set; }

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			bool boolValue = value is bool && (bool)value;

			// Negate if needed
			if (ConverterTools.TryParseBool(parameter))
			{
				boolValue = !boolValue;
			}

			return ConverterTools.Convert(boolValue ? TrueValue : FalseValue, targetType);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			bool result = Equals(value, ConverterTools.Convert(TrueValue, value.GetType()));

			if (ConverterTools.TryParseBool(parameter))
			{
				result = !result;
			}

			return result;
		}
	}

	public partial class BoolToVisibilityConverter : BoolToObjectConverter
	{

		public BoolToVisibilityConverter()
		{
			TrueValue = true;
			FalseValue = false;
		}
	}

	public partial class BoolNegationConverter : IValueConverter
	{

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return !(value is bool && (bool)value);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return !(value is bool && (bool)value);
		}
	}

	public partial class EmptyObjectToObjectConverter : IValueConverter
	{
		
		public object NotEmptyValue { get; set; }

		public object EmptyValue { get; set; }

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			var isEmpty = CheckValueIsEmpty(value);

			// Negate if needed
			if (ConverterTools.TryParseBool(parameter))
			{
				isEmpty = !isEmpty;
			}

			return ConverterTools.Convert(isEmpty ? EmptyValue : NotEmptyValue, targetType);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		protected virtual bool CheckValueIsEmpty(object? value)
		{
			return value == null;
		}
	}

	public partial class EmptyStringToObjectConverter : EmptyObjectToObjectConverter
	{

		protected override bool CheckValueIsEmpty(object? value)
		{
			return string.IsNullOrEmpty(value?.ToString());
		}
	}

	public partial class StringVisibilityConverter : EmptyStringToObjectConverter
	{

		public StringVisibilityConverter()
		{
			NotEmptyValue = true;
			EmptyValue = false;
		}
	}

	public partial class EmptyCollectionToObjectConverter : EmptyObjectToObjectConverter
	{

		protected override bool CheckValueIsEmpty(object? value)
		{
			bool isEmpty = true;
			var collection = value as IEnumerable;
			if (collection != null)
			{
				var enumerator = collection.GetEnumerator();
				isEmpty = !enumerator.MoveNext();
			}

			return isEmpty;
		}
	}

	public partial class FileSizeToFriendlyStringConverter : IValueConverter
	{

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is long size)
			{
				return CommunityToolkit.Common.Converters.ToFileSizeString(size);
			}

			return string.Empty;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
