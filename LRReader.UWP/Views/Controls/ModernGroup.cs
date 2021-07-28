using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "Items")]
	public sealed class ModernGroup : ContentControl
	{
		public ModernGroup()
		{
			this.DefaultStyleKey = typeof(ModernGroup);
			Content = new List<object>();
		}

		public IList<object> Items
		{
			get => GetValue(ContentProperty) as IList<object>;
			set => SetValue(ContentProperty, value);
		}

		public string Title
		{
			get => GetValue(TitleProperty) as string;
			set => SetValue(TitleProperty, value);
		}

		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<object>), typeof(ModernGroup), new PropertyMetadata(null));
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernGroup), new PropertyMetadata(""));
	}

	public class ModernGroupItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate ModernTemplate { get; set; }
		public DataTemplate GenericTemplate { get; set; }
		public DataTemplate InfoBarTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is ModernInput || item is ModernExpander)
				return ModernTemplate;
			if (item is InfoBar)
				return InfoBarTemplate;
			return GenericTemplate;
		}

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}
}
