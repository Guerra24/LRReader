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
			get => (IList<object>)GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		public string Title
		{
			get => (string)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernGroup), new PropertyMetadata(""));
	}
}
