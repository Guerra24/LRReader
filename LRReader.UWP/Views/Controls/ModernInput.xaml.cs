using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Controls
{
	public sealed partial class ModernInput : ContentControl
	{
		public ModernInput()
		{
			this.InitializeComponent();
		}

		public new object Content
		{
			get => GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
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

		public static readonly new DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty SubTextProperty = DependencyProperty.Register("SubText", typeof(string), typeof(ModernInput), new PropertyMetadata(""));
		public static readonly DependencyProperty InputHorizontalAlignmentProperty = DependencyProperty.Register("InputHorizontalAlignment", typeof(HorizontalAlignment), typeof(ModernInput), new PropertyMetadata(HorizontalAlignment.Right));
	}
}
