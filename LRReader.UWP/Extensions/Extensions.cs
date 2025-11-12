using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Animations;
using CommunityToolkit.WinUI.Media;
using LRReader.Shared.Extensions;
using LRReader.Shared.Services;
using LRReader.UWP.Services;
using Markdig;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using WinRT;
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

		//public static void SetVisualTranslation(this UIElement element, Vector3 transform) => ElementCompositionPreview.GetElementVisual(element).TransformMatrix = Matrix4x4.CreateTranslation(transform);

		public static void Start(this UIElement element, AnimationBuilder animation) => animation.Start(element);
		public static Task StartAsync(this UIElement element, AnimationBuilder animation) => animation.StartAsync(element);

	}

	public static class AppExtensions
	{
		public static ElementTheme ToXamlTheme(this AppTheme theme)
		{
			return theme switch
			{
				AppTheme.System => ElementTheme.Default,
				AppTheme.Dark => ElementTheme.Dark,
				AppTheme.Light => ElementTheme.Light,
				_ => throw new NotImplementedException()
			};
		}
		public static FlyoutPlacementMode ToFlyoutPlacement(this TagsPopupLocation location) => location switch
		{
			TagsPopupLocation.Top => FlyoutPlacementMode.RightEdgeAlignedBottom,
			TagsPopupLocation.Middle => FlyoutPlacementMode.Right,
			TagsPopupLocation.Bottom => FlyoutPlacementMode.RightEdgeAlignedTop,
			_ => FlyoutPlacementMode.Auto,
		};
	}

	public class TwoPaneViewExt : DependencyObject
	{
		public static readonly DependencyProperty EnableDualScreenProperty = DependencyProperty.RegisterAttached("EnableDualScreen", typeof(bool), typeof(TwoPaneView), new PropertyMetadata(false));

		public static void SetEnableDualScreen(TwoPaneView pane, bool state)
		{
			if (state)
			{
				var platform = (UWPlatformService)Service.Platform;
				pane.MinWideModeWidth = platform.DualScreenWidth - 10;
			}
			else
				pane.MinWideModeWidth = double.MaxValue;

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


	public static class WebViewExt
	{
		public static readonly DependencyProperty MarkdownProperty = DependencyProperty.RegisterAttached("Markdown", typeof(string), typeof(WebViewExt), new PropertyMetadata(""));
		public static readonly DependencyProperty MarkdownBaseProperty = DependencyProperty.RegisterAttached("MarkdownBase", typeof(string), typeof(WebViewExt), new PropertyMetadata(""));
		public static readonly DependencyProperty MarkdownReadyProperty = DependencyProperty.RegisterAttached("MarkdownReady", typeof(bool), typeof(WebViewExt), new PropertyMetadata(false));
		public static readonly DependencyProperty MarkdownJustifyProperty = DependencyProperty.RegisterAttached("MarkdownJustify", typeof(bool), typeof(WebViewExt), new PropertyMetadata(false));

		private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

		public static void SetMarkdown(this WebView webView, string markdown)
		{
			webView.SetValue(MarkdownProperty, markdown);
			if (!(bool)webView.GetValue(MarkdownReadyProperty))
			{
				webView.ScriptNotify += (sender, args) =>
				{
					if (double.TryParse(args.Value, out var height))
						webView.Height = height;
				};
				webView.SetValue(MarkdownReadyProperty, true);
			}
			webView.SetMarkdownBase(markdown);
		}
		public static string GetMarkdown(WebView webView) => (string)webView.GetValue(MarkdownProperty);

		public static void SetMarkdownBase(this WebView webView, string markdown)
		{
			var color = (Color)Application.Current.Resources["TextFillColorPrimary"];
			var selectedColor = (Color)Application.Current.Resources["TextOnAccentFillColorSelectedText"];
			var selectedBg = (Color)Application.Current.Resources["SystemAccentColor"];
			webView.NavigateToString($$"""
				<!DOCTYPE html>
				<html>
				<head>
					<meta charset="UTF-8">
					<meta name="viewport" content="width=device-width, initial-scale=1.0">
					<title>md</title>
					<style>
						body {
							font-family: "Segoe UI";
							font-size: 14px;
							color: #{{color.R:X2}}{{color.G:X2}}{{color.B:X2}};
							margin: 0;
							{{(webView.GetMarkdownJustify() ? "text-align: justify;" : "")}}
						}
						::selection {
							color: #{{selectedColor.R:X2}}{{selectedColor.G:X2}}{{selectedColor.B:X2}};
							background-color: #{{selectedBg.R:X2}}{{selectedBg.G:X2}}{{selectedBg.B:X2}};
						}
						a {
						    color: #{{selectedBg.R:X2}}{{selectedBg.G:X2}}{{selectedBg.B:X2}};
						}
						img {
							max-width: 80%;
						}
						* {
							box-sizing: border-box;
						}
					</style>
					<script>
						window.addEventListener("resize", () => {
							const rect = document.querySelector('html').getBoundingClientRect();
							window.external.notify(rect.height.toString());
						});
						document.addEventListener("DOMContentLoaded", (event) => {
							const rect = document.querySelector('html').getBoundingClientRect();
							window.external.notify(rect.height.toString());
							requestAnimationFrame(() => {
								const rect = document.querySelector('html').getBoundingClientRect();
								window.external.notify(rect.height.toString());
							});
						});
					</script>
				</head>
				<body>
					{{Markdown.ToHtml(markdown, pipeline)}}
				</body>
				</html>
				""");
			webView.NavigationStarting += (sender, args) =>
			{
				args.Cancel = true;
				Service.Platform.OpenInBrowser(args.Uri);
			};
		}

		public static string GetMarkdownBase(WebView webView) => "";

		public static void SetMarkdownJustify(this WebView webView, bool justify) => webView.SetValue(MarkdownJustifyProperty, justify);

		public static bool GetMarkdownJustify(this WebView webView) => (bool)webView.GetValue(MarkdownJustifyProperty);

	}

	public class Element : DependencyObject
	{
		public static readonly DependencyProperty ModernShadowProperty = DependencyProperty.RegisterAttached("ModernShadow", typeof(Shadow), typeof(Element), new PropertyMetadata(null));

		[DynamicWindowsRuntimeCast(typeof(ListViewBase))]
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
			var item = args.ItemContainer;
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

		[DynamicWindowsRuntimeCast(typeof(GridViewItem))]
		private static void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e) => ((GridViewItem)sender).Translation = new Vector3(0, 0, 32);

		[DynamicWindowsRuntimeCast(typeof(GridViewItem))]
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

	public static class ButtonExtension
	{
		public static readonly DependencyProperty TeachingTipProperty = DependencyProperty.RegisterAttached("TeachingTip", typeof(TeachingTip), typeof(ButtonExtension), new PropertyMetadata(null));

		public static void SetTeachingTip(this Button button, TeachingTip teachingTip)
		{
			button.Click -= Button_Click;
			button.Click += Button_Click;
			teachingTip.Target = button;
			button.SetValue(TeachingTipProperty, teachingTip);
		}

		[DynamicWindowsRuntimeCast(typeof(TeachingTip))]
		public static TeachingTip GetTeachingTip(this Button button) => (TeachingTip)button.GetValue(TeachingTipProperty);

		[DynamicWindowsRuntimeCast(typeof(Button))]
		private static void Button_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			button.GetTeachingTip().IsOpen = true;
		}

		public static readonly DependencyProperty HideFlyoutProperty = DependencyProperty.RegisterAttached("HideFlyout", typeof(Flyout), typeof(ButtonExtension), new PropertyMetadata(null));

		public static void SetHideFlyout(this ButtonBase button, Flyout flyout)
		{
			button.Click += (_, _) => flyout.Hide();
			button.SetValue(HideFlyoutProperty, flyout);
		}

		[DynamicWindowsRuntimeCast(typeof(Flyout))]
		public static Flyout GetHideFlyout(this ButtonBase button) => (Flyout)button.GetValue(HideFlyoutProperty);
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

	[MarkupExtensionReturnType(ReturnType = typeof(string))]
	public partial class LangStringExtension : MarkupExtension
	{
		public string Key { get; set; } = null!;

		protected override object ProvideValue() => Service.Platform.GetLocalizedString(Key);
	}

	/*
	[MarkupExtensionReturnType(ReturnType = typeof(int))]
	public partial class IntExtension : MarkupExtension
	{

		public int Value { get; set; }

		protected override object ProvideValue()
		{
			return Value;
		}
	}

	[MarkupExtensionReturnType(ReturnType = typeof(double))]
	public partial class DoubleExtension : MarkupExtension
	{

		public double Value { get; set; }

		protected override object ProvideValue()
		{
			return Value;
		}
	}

	[MarkupExtensionReturnType(ReturnType = typeof(ClearNewMode))]
	public partial class ClearNewModeEnumExtension : MarkupExtension
	{
		public ClearNewMode Value { get; set; }

		protected override object ProvideValue()
		{
			return Value;
		}
	}*/

	public static class PackageVersionExtension
	{
		public static Version ToVersion(this PackageVersion version) => new Version(version.Major, version.Minor, version.Build, version.Revision);
	}

}
