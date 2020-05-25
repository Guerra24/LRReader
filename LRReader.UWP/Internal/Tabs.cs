using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LRReader.Internal
{
	public class CustomTab : TabViewItem
	{
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

		public static readonly DependencyProperty CustomTabControlProperty = DependencyProperty.RegisterAttached("CustomTabControl", typeof(object), typeof(CustomTab), new PropertyMetadata(null));
		public static readonly DependencyProperty CustomTabIdProperty = DependencyProperty.RegisterAttached("CustomTabId", typeof(object), typeof(CustomTab), new PropertyMetadata(""));
	}
}
