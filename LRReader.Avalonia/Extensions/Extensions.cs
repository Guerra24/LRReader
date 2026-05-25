using Avalonia.Animation.Easings;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using LRReader.Avalonia.Resources;
using LRReader.Shared.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LRReader.Avalonia.Extensions;

public static class Animations
{

	//private static AnimationBuilder FadeIn250 = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(250), easingMode: EasingMode.EaseIn);
	//private static AnimationBuilder FadeOut250 = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(250), easingMode: EasingMode.EaseOut);

	/*public static void FadeIn(this UIElement element) => FadeIn250.Start(element);
	public static void FadeOut(this UIElement element) => FadeOut250.Start(element);
	public static Task FadeInAsync(this UIElement element) => FadeIn250.StartAsync(element);
	public static Task FadeOutAsync(this UIElement element) => FadeOut250.StartAsync(element);*/

	public static void SetVisualOpacity(this Visual element, float opacity)
	{
		var visual = ElementComposition.GetElementVisual(element);
		visual?.Opacity = opacity;
	}

	// Composition opacity does not behave correctly in some cases so fallback to regular opacity
	public static void SetOpacity(this Visual element, float opacity)
	{
		element.Opacity = opacity;
		element.SetVisualOpacity(opacity);
	}

	public static void FadeIn(this Visual element, TimeSpan duration, IEasing easing)
	{
		var visual = ElementComposition.GetElementVisual(element);
		if (visual == null)
			return;
		var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
		animation.InsertKeyFrame(0.0f, 0.0f);
		animation.InsertKeyFrame(1.0f, 1.0f, easing);
		animation.Duration = duration;

		visual.StartAnimation("Opacity", animation);

		// Last frame doesn't invalidate so force it
		Task.Delay(duration).ContinueWith(t =>
		{
			element.Dispatcher.Post(element.InvalidateVisual);
		});
	}

	// Hack
	public static Task FadeInAsync(this Visual element, TimeSpan duration, IEasing easing)
	{
		element.FadeIn(duration, easing);
		return Task.Delay(duration);
	}

	public static void FadeOut(this Visual element, TimeSpan duration, IEasing easing)
	{
		var visual = ElementComposition.GetElementVisual(element);
		if (visual == null)
			return;
		var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
		animation.InsertKeyFrame(0.0f, 1.0f);
		animation.InsertKeyFrame(1.0f, 0.0f, easing);
		animation.Duration = duration;

		visual.StartAnimation("Opacity", animation);

		// Last frame doesn't invalidate so force it
		Task.Delay(duration).ContinueWith(t =>
		{
			element.Dispatcher.Post(element.InvalidateVisual);
		});
	}

	// Hack
	public static Task FadeOutAsync(this Visual element, TimeSpan duration, IEasing easing)
	{
		element.FadeOut(duration, easing);
		return Task.Delay(duration);
	}

	//public static void SetVisualTranslation(this UIElement element, Vector3 transform) => ElementCompositionPreview.GetElementVisual(element).TransformMatrix = Matrix4x4.CreateTranslation(transform);

	/*public static void Start(this UIElement element, AnimationBuilder animation) => animation.Start(element);
	public static Task StartAsync(this UIElement element, AnimationBuilder animation) => animation.StartAsync(element);*/

	public static void SetRepositionAnimation(this Visual element)
	{
		var visual = ElementComposition.GetElementVisual(element);
		var compositor = visual!.Compositor;

		var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
		offsetAnimation.Duration = TimeSpan.FromMilliseconds(300);
		offsetAnimation.Target = "Offset";
		offsetAnimation.InsertExpressionKeyFrame(1f, "this.FinalValue", new ExponentialEaseOut());

		var implicitAnimations = compositor.CreateImplicitAnimationCollection();
		implicitAnimations["Offset"] = offsetAnimation;
		element.Dispatcher.Post(async () =>
		{
			await Task.Yield();
			visual.ImplicitAnimations = implicitAnimations;
		});
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

	public static PlacementMode ToFlyoutPlacement(this TagsPopupLocation location) => location switch
	{
		TagsPopupLocation.Top => PlacementMode.RightEdgeAlignedBottom,
		TagsPopupLocation.Middle => PlacementMode.Right,
		TagsPopupLocation.Bottom => PlacementMode.RightEdgeAlignedTop,
		_ => PlacementMode.Pointer,
	};

}

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

public class ButtonExtension : AvaloniaObject
{
	public static readonly StyledProperty<FATeachingTip> TeachingTipProperty = AvaloniaProperty.RegisterAttached<ButtonExtension, Button, FATeachingTip>("TeachingTip");

	public static void SetTeachingTip(Button button, FATeachingTip teachingTip)
	{
		button.Click -= Button_Click;
		button.Click += Button_Click;
		teachingTip.Target = button;
		button.SetValue(TeachingTipProperty, teachingTip);
	}

	public static FATeachingTip GetTeachingTip(Button button) => button.GetValue(TeachingTipProperty);

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

public static class ImageExtensions
{
	public static SKImage ToSKImage(this Image<Rgba32> image)
	{
		var info = new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Opaque);
		unsafe
		{
			if (image.DangerousTryGetSinglePixelMemory(out var memory))
			{
				using var handle = memory.Pin();
				return SKImage.FromPixelCopy(info, (nint)handle.Pointer);
			}
			else
			{
				var length = info.Width * info.Height * info.BytesPerPixel;
				var temp = NativeMemory.Alloc((nuint)length);
				var span = new Span<byte>(temp, length);
				image.CopyPixelDataTo(span);
				var img = SKImage.FromPixelCopy(info, (nint)temp);
				NativeMemory.Free(temp);
				return img;
			}
		}
	}
}
