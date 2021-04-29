using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using LRReader.Shared.Models;
using System;

namespace LRReader.Avalonia
{
	public class CustomTab : TabItem, ICustomTab, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TabItem);

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
		public bool IsClosable
		{
			get => (bool)GetValue(IsClosableProperty);
			set => SetValue(IsClosableProperty, value);
		}

		public virtual void Unload()
		{
		}

		public static readonly AvaloniaProperty<object> CustomTabControlProperty = AvaloniaProperty.Register<CustomTab, object>("CustomTabControl");
		public static readonly AvaloniaProperty<string> CustomTabIdProperty = AvaloniaProperty.Register<CustomTab, string>("CustomTabId");
		public static readonly AvaloniaProperty<bool> IsClosableProperty = AvaloniaProperty.Register<CustomTab, bool>("IsClosable");
	}
}
