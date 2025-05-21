using System;
using LRReader.Shared.Extensions;
using LRReader.Shared.Services;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Converters
{

	public partial class EnumConverter : IValueConverter
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

	public partial class EnumToInt : IValueConverter
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

	public partial class DisabledTextConverter : IValueConverter
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

	public partial class StringToColorConverter : IValueConverter
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

	public partial class ObjectToBitmapImage : IValueConverter
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

	public partial class TagNamespaceConverter : IValueConverter
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

	//TODO: Fix with MarkupExtension
	public partial class RatingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (parameter == null)
				return false;
			return (double)value == double.Parse((string)parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if ((bool)value)
				return double.Parse((string)parameter);
			else
				return double.NaN;
		}
	}

	public partial class ClearNewEnabledConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (ClearNewMode)value == Enum.Parse<ClearNewMode>((string)parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ArchiveStyleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (ArchiveStyle)value == Enum.Parse<ArchiveStyle>((string)parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(bool)value)
				return ArchiveStyle._InvalidIgnore;
			return Enum.Parse<ArchiveStyle>((string)parameter);
		}
	}
}
