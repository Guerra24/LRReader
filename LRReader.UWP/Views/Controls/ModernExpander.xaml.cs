#nullable enable
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "Items")]
	public sealed partial class ModernExpander : Expander
	{
		public ModernExpander()
		{
			this.InitializeComponent();
			Items = new List<object>();
		}

		public object Input
		{
			get => GetValue(InputProperty);
			set => SetValue(InputProperty, value);
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

		public IList<object> Items
		{
			get => (IList<object>)GetValue(ItemsProperty);
			set => SetValue(ItemsProperty, value);
		}

		public string Glyph
		{
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}

		public IconElement Icon
		{
			get => (IconElement)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		public string ToolTip
		{
			get => (string)GetValue(ToolTipProperty);
			set => SetValue(ToolTipProperty, value);
		}

		public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof(object), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernExpander), new PropertyMetadata(""));
		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ModernExpander), new PropertyMetadata(""));
		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<object>), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register("ToolTip", typeof(string), typeof(ModernExpander), new PropertyMetadata(null));
	}

	public class ItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate LastItem { get; set; } = null!;
		public DataTemplate OtherItem { get; set; } = null!;

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
			return (itemsControl.IndexFromContainer(container) == ((IList<object>)itemsControl.ItemsSource).Count - 1) ? LastItem : OtherItem;
		}
	}
}
