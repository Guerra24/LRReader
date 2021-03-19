using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;

namespace LRReader.UWP.Extensions
{
	public static class Animations
	{

		private static AnimationBuilder FadeIn250 = AnimationBuilder.Create().Opacity(to: 1, duration: TimeSpan.FromMilliseconds(250), easingMode: EasingMode.EaseIn);
		private static AnimationBuilder FadeOut250 = AnimationBuilder.Create().Opacity(to: 0, duration: TimeSpan.FromMilliseconds(250), easingMode: EasingMode.EaseOut);

		public static void FadeIn(this UIElement element) => FadeIn250.Start(element);
		public static void FadeOut(this UIElement element) => FadeOut250.Start(element);
		public static Task FadeInAsync(this UIElement element) => FadeIn250.StartAsync(element);
		public static Task FadeOutAsync(this UIElement element) => FadeOut250.StartAsync(element);

		public static void SetVisualOpacity(this UIElement element, float opacity) => ElementCompositionPreview.GetElementVisual(element).Opacity = opacity;

	}

	public static class StringExtensions
	{
		public static string AsFormat(this string format, params object[] args) => string.Format(format, args);
	}
}
