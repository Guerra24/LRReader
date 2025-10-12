using LRReader.UWP.Installer.Services;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace LRReader.UWP.Installer.Converters;

public partial class InstallStateToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var param = (string)parameter;
		var state = (InstallState)value;
		if (param.StartsWith("!"))
		{
			return state != InstallState.UpgradeAvailable && state != Enum.Parse<InstallState>(param.TrimStart('!')) ? Visibility.Visible : Visibility.Collapsed;
		}
		else
			return state == Enum.Parse<InstallState>(param) ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}

public partial class ProgressBarIsIndeterminateConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		return (double)value == -1;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}