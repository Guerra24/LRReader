using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using LRReader.Avalonia.Resources;
using LRReader.Shared.Services;
using System.Text.RegularExpressions;

namespace LRReader.Avalonia.Extensions;

public sealed partial class LocalizedString : MarkupExtension
{
	public string Key { get; set; } = null!;

	public LocalizedString() { }

	public LocalizedString(string key)
	{
		Key = key;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		var split = MyRegex().Split(Key);
		return ResourceLoader.GetForCurrentView(split[1]).GetString(split[2]);
	}

	[GeneratedRegex(@"\/(.*?)\/(.*)")]
	private static partial Regex MyRegex();
}

public sealed class EnumValue : MarkupExtension
{

	public Type Type { get; set; } = null!;

	public string Member { get; set; } = null!;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return Enum.Parse(Type, Member);
	}
}

public static class AppExtensions
{
	public static ThemeVariant ToXamlTheme(this AppTheme theme)
	{
		return theme switch
		{
			AppTheme.System => ThemeVariant.Default,
			AppTheme.Dark => ThemeVariant.Dark,
			AppTheme.Light => ThemeVariant.Light,
			_ => throw new NotImplementedException()
		};
	}

}

public class ButtonExtension : AvaloniaObject
{
	public static readonly StyledProperty<TeachingTip> TeachingTipProperty = AvaloniaProperty.RegisterAttached<ButtonExtension, Button, TeachingTip>("TeachingTip");

	public static void SetTeachingTip(Button button, TeachingTip teachingTip)
	{
		button.Click -= Button_Click;
		button.Click += Button_Click;
		teachingTip.Target = button;
		button.SetValue(TeachingTipProperty, teachingTip);
	}

	public static TeachingTip GetTeachingTip(Button button) => button.GetValue(TeachingTipProperty);

	private static void Button_Click(object? sender, RoutedEventArgs e)
	{
		var button = (Button)sender!;
		GetTeachingTip(button).IsOpen = true;
	}

	public static readonly StyledProperty<Flyout> HideFlyoutProperty = AvaloniaProperty.RegisterAttached<ButtonExtension, Button, Flyout>("HideFlyout");

	public static void SetHideFlyout(Button button, Flyout flyout)
	{
		button.Click += (_, _) => flyout.Hide();
		button.SetValue(HideFlyoutProperty, flyout);
	}

	public static Flyout GetHideFlyout(Button button) => button.GetValue(HideFlyoutProperty);
}
