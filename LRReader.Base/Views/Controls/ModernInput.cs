#if WINDOWS_UWP
using PointerEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using PointerPressedEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using PointerReleasedEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using FocusChangedEventArgs = Windows.UI.Xaml.RoutedEventArgs;
using Key = Windows.System.VirtualKey;

namespace LRReader.UWP.Views.Controls;
#else
using Avalonia.Input;
using Avalonia.Interactivity;

namespace LRReader.Avalonia.Views.Controls;
#endif

public sealed partial class ModernInput : ContentControl
{

	protected override void OnPointerEntered(PointerEventArgs e)
	{
		base.OnPointerEntered(e);
#if WINDOWS_UWP
		if (IsButton && IsEnabled)
			VisualStateManager.GoToState(this, "PointerOver", true);
#endif
	}

	protected override void OnPointerExited(PointerEventArgs e)
	{
		base.OnPointerExited(e);
#if WINDOWS_UWP
		if (IsButton && IsEnabled)
			VisualStateManager.GoToState(this, "Normal", true);
#endif
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		var point = e.GetCurrentPoint(this);
		if (!point.Properties.IsLeftButtonPressed)
			return;
		base.OnPointerPressed(e);
		if (IsButton && IsEnabled)
#if WINDOWS_UWP
			VisualStateManager.GoToState(this, "Pressed", true);
#else
			PseudoClasses.Set(":pressed", true);
#endif
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		var point = e.GetCurrentPoint(this);
		if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
			return;
		base.OnPointerReleased(e);
		if (IsButton && IsEnabled)
		{
			if (Command != null && Command.CanExecute(CommandParameter))
				Command.Execute(CommandParameter);
#if WINDOWS_UWP
			Click?.Invoke(this, e);
			VisualStateManager.GoToState(this, "PointerOver", true);
#else
			RaiseEvent(new RoutedEventArgs(ClickEvent));
			PseudoClasses.Set(":pressed", false);
#endif
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (IsButton && IsEnabled && (e.Key == Key.Space || e.Key == Key.Enter))
#if WINDOWS_UWP
			VisualStateManager.GoToState(this, "Pressed", true);
#else
			PseudoClasses.Set(":pressed", true);
#endif
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		if (IsButton && IsEnabled && (e.Key == Key.Space || e.Key == Key.Enter))
		{
#if WINDOWS_UWP
			Click?.Invoke(this, e);
			VisualStateManager.GoToState(this, "Normal", true);
#else
			RaiseEvent(new RoutedEventArgs(ClickEvent));
			PseudoClasses.Set(":pressed", false);
#endif
		}
	}

	protected override void OnLostFocus(FocusChangedEventArgs e)
	{
		base.OnLostFocus(e);
		if (IsButton && IsEnabled)
#if WINDOWS_UWP
			VisualStateManager.GoToState(this, "Normal", true);
#else
			PseudoClasses.Set(":pressed", false);
#endif
	}
}
