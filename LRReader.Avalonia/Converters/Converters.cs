using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using LRReader.Shared.Extensions;
using LRReader.Shared.Services;
using System.Globalization;

namespace LRReader.Avalonia.Converters
{

	public partial class EnumConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			string? parameterString = parameter as string;
			if (parameterString == null || !Enum.IsDefined(value!.GetType(), value))
				return null;
			object parameterValue = Enum.Parse(value.GetType(), parameterString);

			return parameterValue.Equals(value);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			string? parameterString = parameter as string;
			if (parameterString == null)
				return null;
			return Enum.Parse(targetType, parameterString);
		}
	}

	public partial class EnumToInt : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return (int)value!;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return Enum.ToObject(targetType, value!);
		}
	}

	public partial class DisabledTextConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return (bool)value! ? Application.Current!.Resources["TextControlHeaderForegroundDisabled"] : Application.Current!.Resources["TextControlHeaderForeground"];
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ObjectToBitmapImage : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is Bitmap image)
				return image;
			return null!;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public partial class TagNamespaceConverter : IValueConverter
	{

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return ((string)value!).UpperFirstLetter().Replace('_', ' ');
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	//TODO: Fix with MarkupExtension
	public partial class RatingConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (parameter == null)
				return false;
			return (double)value! == double.Parse((string)parameter);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if ((bool)value!)
				return double.Parse((string)parameter!);
			else
				return double.NaN;
		}
	}

	public partial class ClearNewEnabledConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return (ClearNewMode)value! == Enum.Parse<ClearNewMode>((string)parameter!);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public partial class ArchiveStyleConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return (ArchiveStyle)value! == Enum.Parse<ArchiveStyle>((string)parameter!);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return Enum.Parse<ArchiveStyle>((string)parameter!);
		}
	}

	public partial class StringToColorConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return Color.Parse(value!.ToString()!);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return value!.ToString();
		}
	}
}
