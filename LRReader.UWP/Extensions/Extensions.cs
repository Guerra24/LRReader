using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
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

	public class ElementsShadow : DependencyObject
	{
		public static readonly DependencyProperty ShadowProperty = DependencyProperty.RegisterAttached("Shadow", typeof(object), typeof(ElementsShadow), new PropertyMetadata(null));

		public static void SetShadow(UIElement element, object shadow)
		{
			element.SetValue(ShadowProperty, shadow);
			if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
			{
				if (element is GridView grid)
				{
					grid.ContainerContentChanging += Grid_ContainerContentChanging;
				}
				else if (element.Shadow == null)
				{
					element.Shadow = shadow as ThemeShadow;
					element.Translation = new Vector3(0, 0, 14);
				}
			}
			else
			{
				// TODO Shadows non11
			}
		}

		public static object GetShadow(UIElement element)
		{
			return element.GetValue(ShadowProperty);
		}

		private static void Grid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			var itemShadow = (args.ItemContainer as GridViewItem).Shadow;
			if (itemShadow == null)
			{
				(args.ItemContainer as GridViewItem).Shadow = sender.GetValue(ShadowProperty) as ThemeShadow;
				(args.ItemContainer as GridViewItem).Translation = new Vector3(0, 0, 14);
			}
		}
	}

	public static class PopupExtension
	{

		public static void ClampPopup(this Popup popup, double horizontalOffset)
		{
			var point = popup.TransformToVisual(Window.Current.Content).TransformPoint(new Point(horizontalOffset, 0));
			if (point.X <= 0)
				popup.HorizontalOffset = Math.Max(horizontalOffset, horizontalOffset - point.X);
			else if (point.X + 310 > Window.Current.Bounds.Width)
				popup.HorizontalOffset = Math.Min(horizontalOffset, horizontalOffset - (point.X + 310 - Window.Current.Bounds.Width));
			else
				popup.HorizontalOffset = horizontalOffset;
		}
	}
}
