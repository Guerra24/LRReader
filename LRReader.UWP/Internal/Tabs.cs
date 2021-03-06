﻿using LRReader.Shared.Models;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace LRReader.Internal
{
	public class CustomTab : TabViewItem, ICustomTab
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

		public static readonly DependencyProperty CustomTabControlProperty = DependencyProperty.Register("CustomTabControl", typeof(object), typeof(CustomTab), new PropertyMetadata(null));
		public static readonly DependencyProperty CustomTabIdProperty = DependencyProperty.Register("CustomTabId", typeof(string), typeof(CustomTab), new PropertyMetadata(""));
	}
}
