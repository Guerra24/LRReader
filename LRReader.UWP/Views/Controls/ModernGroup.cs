﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

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

		public string Text
		{
			get => GetValue(TextProperty) as string;
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<object>), typeof(ModernGroup), new PropertyMetadata(null));
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ModernGroup), new PropertyMetadata(""));
	}
}
