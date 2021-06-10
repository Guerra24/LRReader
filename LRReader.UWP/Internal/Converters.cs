using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace LRReader.Internal
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public BooleanToVisibilityConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is bool && (bool)value)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return (value is Visibility && (Visibility)value == Visibility.Visible);
		}
	}

	public class NullToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class NegateBoolConverter : IValueConverter
	{
		public NegateBoolConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
	public class NegateBoolToVisibilityConverter : IValueConverter
	{
		public NegateBoolToVisibilityConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is bool && (bool)value)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return (value is Visibility && (Visibility)value == Visibility.Collapsed);
		}
	}
	public class EnumConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string parameterString = parameter as string;
			if (parameterString == null || !Enum.IsDefined(value.GetType(), value))
				return DependencyProperty.UnsetValue;
			object parameterValue = Enum.Parse(value.GetType(), parameterString);

			return parameterValue.Equals(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			string parameterString = parameter as string;
			if (parameterString == null)
				return DependencyProperty.UnsetValue;
			return Enum.Parse(targetType, parameterString);
		}
	}
	public class EnumToInt : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (int)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return Enum.ToObject(targetType, value);
		}
	}
	public class DisabledTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (bool)value ? Application.Current.Resources["TextControlHeaderForegroundDisabled"] : Application.Current.Resources["TextControlHeaderForeground"];
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
