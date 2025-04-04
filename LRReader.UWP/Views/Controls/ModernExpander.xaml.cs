using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
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

		[GeneratedDependencyProperty]
		public partial object? Input { get; set; }

		[GeneratedDependencyProperty(DefaultValue = "")]
		public partial string Title { get; set; }

		[GeneratedDependencyProperty(DefaultValue = "")]
		public partial string Description { get; set; }

		[GeneratedDependencyProperty]
		public partial IList<object>? Items { get; set; }

		[GeneratedDependencyProperty]
		public partial string? Glyph { get; set; }

		[GeneratedDependencyProperty]
		public partial IconElement? Icon { get; set; }

		[GeneratedDependencyProperty]
		public partial string? ToolTip { get; set; }
	}

	public partial class ItemTemplateSelector : DataTemplateSelector
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
