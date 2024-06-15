using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Controls
{
	public class ModernTabView : TabView
	{

		public object ExtraFooter
		{
			get => GetValue(ExtraFooterProperty);
			set => SetValue(ExtraFooterProperty, value);
		}

		public static readonly DependencyProperty ExtraFooterProperty = DependencyProperty.Register("ExtraFooter", typeof(object), typeof(ModernTabView), new PropertyMetadata(null));
	}
}
