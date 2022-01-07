using Aura.UI.Controls;
using Avalonia;
using Avalonia.Styling;
using LRReader.Shared.Models;
using System;

namespace LRReader.Avalonia
{
	public class CustomTab : AuraTabItem, ICustomTab, IStyleable
	{
		Type IStyleable.StyleKey => typeof(AuraTabItem);

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

		public virtual bool BackRequested()
		{
			return false;
		}

		public static readonly AvaloniaProperty<object> CustomTabControlProperty = AvaloniaProperty.Register<CustomTab, object>("CustomTabControl");
		public static readonly AvaloniaProperty<string> CustomTabIdProperty = AvaloniaProperty.Register<CustomTab, string>("CustomTabId");
	}
}
