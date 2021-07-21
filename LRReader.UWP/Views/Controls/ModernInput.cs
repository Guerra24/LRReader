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

		public string Title
		{
			get => GetValue(TitleProperty) as string;
			set => SetValue(TitleProperty, value);
		}
		public string Description
		{
			get => GetValue(DescriptionProperty) as string;
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
			get => GetValue(GlyphProperty) as string;
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
			set => SetValue(IsButtonProperty, value);
		}

		public string RightGlyph
		{
			get => GetValue(RightGlyphProperty) as string;
			set => SetValue(RightGlyphProperty, value);
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
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("DescriptionProperty", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty InputHorizontalAlignmentProperty = DependencyProperty.Register("InputHorizontalAlignment", typeof(HorizontalAlignment), typeof(ModernInput), new PropertyMetadata(HorizontalAlignment.Right));
		public static readonly DependencyProperty TextMarginProperty = DependencyProperty.Register("TextMargin", typeof(Thickness), typeof(ModernInput), new PropertyMetadata(new Thickness(0)));
		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty IsButtonProperty = DependencyProperty.Register("IsButton", typeof(bool), typeof(ModernInput), new PropertyMetadata(false));
		public static readonly DependencyProperty RightGlyphProperty = DependencyProperty.Register("RightGlyph", typeof(string), typeof(ModernInput), new PropertyMetadata(null));
	}
}
