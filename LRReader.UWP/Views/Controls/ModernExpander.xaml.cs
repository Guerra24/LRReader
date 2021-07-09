using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
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

		public IList<object> Items
		{
			get => GetValue(ItemsProperty) as IList<object>;
			set => SetValue(ItemsProperty, value);
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

		public string ToolTip
		{
			get => GetValue(ToolTipProperty) as string;
			set => SetValue(ToolTipProperty, value);
		}

		public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof(object), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(ModernExpander), new PropertyMetadata(""));
		public static readonly DependencyProperty SubTextProperty = DependencyProperty.Register("SubText", typeof(string), typeof(ModernExpander), new PropertyMetadata(""));
		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<object>), typeof(ModernExpander), new PropertyMetadata(new List<object>()));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty CustomIconProperty = DependencyProperty.Register("CustomIcon", typeof(object), typeof(ModernInput), new PropertyMetadata(null));
		public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register("ToolTip", typeof(string), typeof(ModernExpander), new PropertyMetadata(null));
	}

	public class ItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate LastItem { get; set; }
		public DataTemplate OtherItem { get; set; }

		public IList<object> Items { get; set; }
		public ItemsRepeater ItemsRepeater { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			var last = Items.LastOrDefault();
			if (item.Equals(last))
				return LastItem;
			return OtherItem;
		}
	}
}
