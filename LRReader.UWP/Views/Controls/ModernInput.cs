using CommunityToolkit.WinUI;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "Control")]
	public sealed partial class ModernInput : ContentControl
	{
		public ModernInput()
		{
			this.DefaultStyleKey = typeof(ModernInput);
			IsEnabledChanged += ModernInput_IsEnabledChanged;
		}

		private void ModernInput_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
				VisualStateManager.GoToState(this, "Normal", true);
			else
				VisualStateManager.GoToState(this, "Disabled", true);
		}

		[GeneratedDependencyProperty]
		public partial object? Control { get; set; }

		[GeneratedDependencyProperty(DefaultValue = "")]
		public partial string Title { get; set; }
		[GeneratedDependencyProperty(DefaultValue = "")]
		public partial string Description { get; set; }

		[GeneratedDependencyProperty(DefaultValue = HorizontalAlignment.Right)]
		public partial HorizontalAlignment InputHorizontalAlignment { get; set; }

		[GeneratedDependencyProperty(DefaultValueCallback = "GetDefaultThickness")]
		public partial Thickness TextMargin { get; set; }

		private static object GetDefaultThickness() => new Thickness(0);

		[GeneratedDependencyProperty]
		public partial string? Glyph { get; set; }

		[GeneratedDependencyProperty]
		public partial object? Icon { get; set; }

		[GeneratedDependencyProperty]
		public partial bool IsButton { get; set; }

		partial void OnIsButtonChanged(bool newValue) => IsTabStop = newValue;

		[GeneratedDependencyProperty]
		public partial string? RightGlyph { get; set; }

		[GeneratedDependencyProperty]
		public partial ICommand? Command { get; set; }
		[GeneratedDependencyProperty]
		public partial object? CommandParameter { get; set; }

		[GeneratedDependencyProperty(DefaultValueCallback = "GetDefaultRightColumnWidth")]
		public partial GridLength RightColumnWidth { get; set; }

		private static object GetDefaultRightColumnWidth() => GridLength.Auto;

		public event RoutedEventHandler? Click;

		protected override void OnPointerEntered(PointerRoutedEventArgs e)
		{
			base.OnPointerEntered(e);
			if (IsButton && IsEnabled)
				VisualStateManager.GoToState(this, "PointerOver", true);
		}

		protected override void OnPointerExited(PointerRoutedEventArgs e)
		{
			base.OnPointerExited(e);
			if (IsButton && IsEnabled)
				VisualStateManager.GoToState(this, "Normal", true);
		}

		protected override void OnPointerPressed(PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(this);
			if (!point.Properties.IsLeftButtonPressed)
				return;
			base.OnPointerPressed(e);
			if (IsButton && IsEnabled)
				VisualStateManager.GoToState(this, "Pressed", true);
		}

		protected override void OnPointerReleased(PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(this);
			if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
				return;
			base.OnPointerReleased(e);
			if (IsButton && IsEnabled)
			{
				if (Command != null && Command.CanExecute(CommandParameter))
					Command.Execute(CommandParameter);
				Click?.Invoke(this, e);
				VisualStateManager.GoToState(this, "PointerOver", true);
			}
		}

		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			base.OnKeyDown(e);
			if (IsButton && IsEnabled && (e.Key == VirtualKey.Space || e.Key == VirtualKey.Enter))
				VisualStateManager.GoToState(this, "Pressed", true);
		}

		protected override void OnKeyUp(KeyRoutedEventArgs e)
		{
			base.OnKeyUp(e);
			if (IsButton && IsEnabled && (e.Key == VirtualKey.Space || e.Key == VirtualKey.Enter))
			{
				Click?.Invoke(this, e);
				VisualStateManager.GoToState(this, "Normal", true);
			}
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			if (IsButton && IsEnabled)
				VisualStateManager.GoToState(this, "Normal", true);
		}
	}
}
