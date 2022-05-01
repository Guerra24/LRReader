using Aura.UI.Controls;
using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using LRReader.Shared.Models;
using System;

namespace LRReader.Avalonia
{
	public class CustomTab : AuraTabItem, ICustomTab, IStyleable
	{
		Type IStyleable.StyleKey => typeof(CustomTab);

		public Symbol CustomTabIcon
		{
			get => (Symbol)GetValue(CustomTabIconProperty)!;
			set => SetValue(CustomTabIconProperty, value);
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

		public bool CustomTabIsClosable
		{
			get => (bool)GetValue(CustomTabIsClosableProperty);
			set => SetValue(CustomTabIsClosableProperty, value);
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
		public static readonly AvaloniaProperty<Symbol> CustomTabIconProperty = AvaloniaProperty.Register<CustomTab, Symbol>("CustomTabIcon");
		public static readonly AvaloniaProperty<bool> CustomTabIsClosableProperty = AvaloniaProperty.Register<CustomTab, bool>("CustomTabIsClosable", true);
	}
}
