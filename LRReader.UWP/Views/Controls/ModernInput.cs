using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "Control")]
	public sealed class ModernInput : ContentControl
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

		public string HeaderText
		{
			get => GetValue(HeaderTextProperty) as string;
			set => SetValue(HeaderTextProperty, value);
		}

		public string SubText
		{
			get => GetValue(SubTextProperty) as string;
			set => SetValue(SubTextProperty, value);
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

		public string Icon
		{
			get => GetValue(IconProperty) as string;
			set => SetValue(IconProperty, value);
		}

		public object CustomIcon
		{
			get => GetValue(CustomIconProperty);
			set => SetValue(CustomIconProperty, value);
		}

		public bool IsButton
		{
			get => (bool)GetValue(IsButtonProperty);
			set => SetValue(IsButtonProperty, value);
		}

		public string RightIcon
		{
			get => GetValue(RightIconProperty) as string;
			set => SetValue(RightIconProperty, value);
		}

		public event RoutedEventHandler Click;

		protected override void OnPointerEntered(PointerRoutedEventArgs e)
		{
			base.OnPointerEntered(e);
			if (IsButton)
				VisualStateManager.GoToState(this, "PointerOver", true);
		}

		protected override void OnPointerExited(PointerRoutedEventArgs e)
		{
			base.OnPointerExited(e);
			if (IsButton)
				VisualStateManager.GoToState(this, "Normal", true);
		}

		protected override void OnPointerPressed(PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(this);
			if (!point.Properties.IsLeftButtonPressed)
				return;
			base.OnPointerPressed(e);
			if (IsButton)
				VisualStateManager.GoToState(this, "Pressed", true);
		}

		protected override void OnPointerReleased(PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(this);
			if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
				return;
			base.OnPointerReleased(e);
			if (IsButton)
			{
				Click?.Invoke(this, null);
				VisualStateManager.GoToState(this, "PointerOver", true);
			}
		}

		public static readonly DependencyProperty ControlProperty = DependencyProperty.Register("Control", typeof(object), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty SubTextProperty = DependencyProperty.Register("SubText", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty InputHorizontalAlignmentProperty = DependencyProperty.Register("InputHorizontalAlignment", typeof(HorizontalAlignment), typeof(ModernInput), new PropertyMetadata(HorizontalAlignment.Right));
		public static readonly DependencyProperty TextMarginProperty = DependencyProperty.Register("TextMargin", typeof(Thickness), typeof(ModernInput), new PropertyMetadata(new Thickness(0)));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty CustomIconProperty = DependencyProperty.Register("CustomIcon", typeof(object), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty IsButtonProperty = DependencyProperty.Register("IsButton", typeof(bool), typeof(ModernInput), new PropertyMetadata(false));
		public static readonly DependencyProperty RightIconProperty = DependencyProperty.Register("RightIcon", typeof(string), typeof(ModernInput), new PropertyMetadata(null));
	}
}
