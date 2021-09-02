using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Media;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
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

	public class Element : DependencyObject
	{
		public static readonly DependencyProperty ModernShadowProperty = DependencyProperty.RegisterAttached("ModernShadow", typeof(Shadow), typeof(Element), new PropertyMetadata(null));

		public static void SetModernShadow(FrameworkElement element, Shadow shadow)
		{
			element.SetValue(ModernShadowProperty, shadow);
			if (element is ListViewBase grid)
			{
				grid.ContainerContentChanging += Grid_ContainerContentChanging;
			}
			else
			{
				if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
				{
					element.Shadow = shadow.ThemeShadow;
					element.Translation = new Vector3(0, 0, 14);
				}
				else
				{
					Effects.SetShadow(element, shadow.DropShadow);
				}
			}
		}

		public static Shadow GetModernShadow(FrameworkElement element) => element.GetValue(ModernShadowProperty) as Shadow;

		private static void Grid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			var item = args.ItemContainer as GridViewItem;
			var shadow = sender.GetValue(ModernShadowProperty) as Shadow;
			if (item.GetValue(ModernShadowProperty) == null)
			{
				item.SetValue(ModernShadowProperty, shadow);
				if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
				{
					item.Shadow = shadow.ThemeShadow;
					item.Translation = new Vector3(0, 0, 14);
					item.TranslationTransition = new Vector3Transition();
					item.TranslationTransition.Duration = TimeSpan.FromMilliseconds(100);
					item.PointerEntered += GridViewItem_PointerEntered;
					item.PointerExited += GridViewItem_PointerExited;
				}
				else
				{
					Effects.SetShadow(item, shadow.DropShadow);
				}
			}
		}

		private static void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e) => (sender as GridViewItem).Translation = new Vector3(0, 0, 32);

		private static void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e) => (sender as GridViewItem).Translation = new Vector3(0, 0, 14);
	}

	public class Shadow
	{
		internal ThemeShadow ThemeShadow { get; }
		internal AttachedCardShadow DropShadow { get; }

		public Shadow()
		{
			if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
				ThemeShadow = new ThemeShadow();
			else
				DropShadow = new AttachedCardShadow { BlurRadius = 8, CornerRadius = 4, Color = Colors.Black, Offset = "0,2", Opacity = 0.15 };
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
