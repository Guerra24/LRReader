using LRReader.Shared.Models;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Controls
{
	public class ModernTab : TabViewItem, ICustomTab
	{
		public ModernTab()
		{
			//this.DefaultStyleKey = typeof(ModernTab);
		}

		public object CustomTabControl
		{
			get => GetValue(CustomTabControlProperty);
			set => SetValue(CustomTabControlProperty, value);
		}

		public string CustomTabId
		{
			get => GetValue(CustomTabIdProperty) as string;
			set => SetValue(CustomTabIdProperty, value);
		}

		public virtual void Unload()
		{
		}

		public static readonly DependencyProperty CustomTabControlProperty = DependencyProperty.Register("CustomTabControl", typeof(object), typeof(ModernTab), new PropertyMetadata(null));
		public static readonly DependencyProperty CustomTabIdProperty = DependencyProperty.Register("CustomTabId", typeof(string), typeof(ModernTab), new PropertyMetadata(""));
	}
}
