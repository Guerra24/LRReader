using Avalonia;
using FluentAvalonia.UI.Controls;
using LRReader.Shared.Models;
using System;

namespace LRReader.Avalonia.Views.Controls
{
	public class CustomTab : TabViewItem, ICustomTab
	{
		protected override Type StyleKeyOverride => typeof(CustomTab);

		public object? CustomTabControl
		{
			get => GetValue(CustomTabControlProperty);
			set => SetValue(CustomTabControlProperty, value);
		}

		public string CustomTabId
		{
			get => (string)GetValue(CustomTabIdProperty)!;
			set => SetValue(CustomTabIdProperty, value);
		}

		public Shared.Services.Tab Tab { get; set; }
		public virtual TabState GetTabState() => new TabState(Tab);

		public virtual bool BackRequested()
		{
			return false;
		}

		public virtual void Dispose()
		{
		}

		public static readonly AvaloniaProperty<object?> CustomTabControlProperty = AvaloniaProperty.Register<CustomTab, object?>("CustomTabControl");
		public static readonly AvaloniaProperty<string> CustomTabIdProperty = AvaloniaProperty.Register<CustomTab, string>("CustomTabId");
	}
}
