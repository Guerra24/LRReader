using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using LRReader.Shared.Extensions;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Media;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using TwoPaneView = Microsoft.UI.Xaml.Controls.TwoPaneView;

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

		public static void Start(this UIElement element, AnimationBuilder animation) => animation.Start(element);
		public static Task StartAsync(this UIElement element, AnimationBuilder animation) => animation.StartAsync(element);

	}

	public class TwoPaneViewExt : DependencyObject
	{
		public static readonly DependencyProperty EnableDualScreenProperty = DependencyProperty.RegisterAttached("EnableDualScreen", typeof(bool), typeof(TwoPaneView), new PropertyMetadata(false));

		public static void SetEnableDualScreen(TwoPaneView pane, bool state)
		{
			pane.MinWideModeWidth = state ? 1340 : double.MaxValue;
			pane.SetValue(EnableDualScreenProperty, state);
		}

		public static bool GetEnableDualScreen(TwoPaneView pane) => (bool)pane.GetValue(EnableDualScreenProperty);
	}

	public class GridViewExt : DependencyObject
	{
		public static readonly DependencyProperty ItemClickCommandProperty = DependencyProperty.RegisterAttached("ItemClickCommand", typeof(ICommand), typeof(GridViewExt), new PropertyMetadata(null));

		public static void SetItemClickCommand(GridView gridView, ICommand command)
		{
			gridView.SetValue(ItemClickCommandProperty, command);
			gridView.ItemClick += (sender, e) =>
			{
				if (command.CanExecute(new GridViewExtParameter(false, e.ClickedItem)))
					command.Execute(new GridViewExtParameter((CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down, e.ClickedItem));
			};
		}

		public static ICommand GetItemClickCommand(GridView gridView) => (ICommand)gridView.GetValue(ItemClickCommandProperty);
	}

	public class KeyboardAcceleratorExt : DependencyObject
	{
		public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(KeyboardAcceleratorExt), new PropertyMetadata(null));
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(KeyboardAcceleratorExt), new PropertyMetadata(null));

		public static void SetCommand(KeyboardAccelerator ka, ICommand command)
		{
			ka.SetValue(CommandProperty, command);
			ka.Invoked += (sender, e) =>
			{
				if (command.CanExecute(GetCommandParameter(ka)))
				{
					e.Handled = true;
					command.Execute(GetCommandParameter(ka));
				}
			};
		}

		public static ICommand GetCommand(KeyboardAccelerator ka) => (ICommand)ka.GetValue(CommandProperty);

		public static void SetCommandParameter(KeyboardAccelerator ka, object parameter) => ka.SetValue(CommandParameterProperty, parameter);

		public static object GetCommandParameter(KeyboardAccelerator ka) => ka.GetValue(CommandParameterProperty);
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
				/*if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
				{
					element.Shadow = shadow.ThemeShadow;
					element.Translation = shadow.Translation;
				}
				else
				{*/
				Effects.SetShadow(element, shadow.DropShadow);
				//}
			}
		}

		public static Shadow GetModernShadow(FrameworkElement element) => (Shadow)element.GetValue(ModernShadowProperty);

		private static void Grid_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			var item = (GridViewItem)args.ItemContainer;
			var shadow = (Shadow)sender.GetValue(ModernShadowProperty);
			if (item.GetValue(ModernShadowProperty) == null)
			{
				item.SetValue(ModernShadowProperty, shadow);
				/*if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
				{
					item.Shadow = shadow.ThemeShadow;
					item.Translation = shadow.Translation;
					item.TranslationTransition = new Vector3Transition();
					item.TranslationTransition.Duration = TimeSpan.FromMilliseconds(100);
					item.PointerEntered += GridViewItem_PointerEntered;
					item.PointerExited += GridViewItem_PointerExited;
				}
				else
				{*/
				Effects.SetShadow(item, shadow.DropShadow);
				//}
			}
		}

		private static void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e) => ((GridViewItem)sender).Translation = new Vector3(0, 0, 32);

		private static void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e) => ((GridViewItem)sender).Translation = new Vector3(0, 0, 14);
	}

	public class Shadow
	{
		internal ThemeShadow ThemeShadow { get; } = null!;
		internal AttachedCardShadow DropShadow { get; } = null!;

		public Vector3 Translation { get; set; } = new Vector3(0, 0, 14);

		public Shadow()
		{
			/*if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 13))
				ThemeShadow = new ThemeShadow();
			else*/
			DropShadow = new AttachedCardShadow { BlurRadius = 8, CornerRadius = 4, Color = Colors.Black, Offset = "0,2", Opacity = 0.16 };
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
