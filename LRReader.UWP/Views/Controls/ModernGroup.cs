using CommunityToolkit.WinUI;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "Items")]
	public sealed partial class ModernGroup : ContentControl
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

		[GeneratedDependencyProperty(DefaultValue = "")]
		public partial string Title { get; set; }
	}
}
