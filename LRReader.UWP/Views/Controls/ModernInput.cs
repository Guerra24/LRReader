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
		}

		public object Control
		{
			get => GetValue(ControlProperty);
			set => SetValue(ControlProperty, value);
		}

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}
		public string Description
		{
			get => (string)GetValue(DescriptionProperty);
			set => SetValue(DescriptionProperty, value);
		}

		public HorizontalAlignment InputHorizontalAlignment
		{
			get => (HorizontalAlignment)GetValue(InputHorizontalAlignmentProperty);
			set => SetValue(InputHorizontalAlignmentProperty, value);
		}

		public Thickness TextMargin
		{
			get => (Thickness)GetValue(TextMarginProperty);
			set => SetValue(TextMarginProperty, value);
		}

		public string Glyph
		{
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}

		public object Icon
		{
			get => GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		public bool IsButton
		{
			get => (bool)GetValue(IsButtonProperty);
			set => SetValue(IsButtonProperty, IsTabStop = value);
		}

		public string RightGlyph
		{
			get => (string)GetValue(RightGlyphProperty);
			set => SetValue(RightGlyphProperty, value);
		}

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public GridLength RightColumnWidth
		{
			get => (GridLength)GetValue(RightColumnWidthProperty);
			set
			{
				SetValue(RightColumnWidthProperty, value);
			}
		}

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

		public static readonly DependencyProperty ControlProperty = DependencyProperty.Register("Control", typeof(object), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty InputHorizontalAlignmentProperty = DependencyProperty.Register("InputHorizontalAlignment", typeof(HorizontalAlignment), typeof(ModernInput), new PropertyMetadata(HorizontalAlignment.Right));
		public static readonly DependencyProperty TextMarginProperty = DependencyProperty.Register("TextMargin", typeof(Thickness), typeof(ModernInput), new PropertyMetadata(new Thickness(0)));
		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty IsButtonProperty = DependencyProperty.Register("IsButton", typeof(bool), typeof(ModernInput), new PropertyMetadata(false));
		public static readonly DependencyProperty RightGlyphProperty = DependencyProperty.Register("RightGlyph", typeof(string), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty RightColumnWidthProperty = DependencyProperty.Register("RightColumnWidth", typeof(GridLength), typeof(ModernInput), new PropertyMetadata(GridLength.Auto));
	}
}
