#nullable enable
using System;
using LRReader.Shared.Extensions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Converters
{

	public class EnumConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string? parameterString = parameter as string;
			if (parameterString == null || !Enum.IsDefined(value.GetType(), value))
				return DependencyProperty.UnsetValue;
			object parameterValue = Enum.Parse(value.GetType(), parameterString);

			return parameterValue.Equals(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			string? parameterString = parameter as string;
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

	public class StringToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is string colorString)
			{
				colorString = colorString.Replace("#", string.Empty);
				var r = (byte)System.Convert.ToUInt32(colorString.Substring(0, 2), 16);
				var g = (byte)System.Convert.ToUInt32(colorString.Substring(2, 2), 16);
				var b = (byte)System.Convert.ToUInt32(colorString.Substring(4, 2), 16);
				return new SolidColorBrush(Color.FromArgb(255, r, g, b));
			}
			else
			{
				return Application.Current.Resources["TextControlForeground"];
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class ObjectToBitmapImage : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is BitmapImage image)
				return image;
			return null!;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class TagNamespaceConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return ((string)value).UpperFirstLetter().Replace('_', ' ');
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class RatingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (double)value == (double)parameter;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if ((bool)value)
				return parameter;
			else
				return double.NaN;
		}
	}
}
