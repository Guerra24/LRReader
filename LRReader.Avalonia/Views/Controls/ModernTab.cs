using FluentAvalonia.UI.Controls;
using LRReader.Shared.Models;

namespace LRReader.Avalonia.Views.Controls
{
	public class ModernTab : FATabViewItem, ICustomTab
	{
		protected override Type StyleKeyOverride => typeof(ModernTab);

		public object? CustomTabControl
		{
			get => GetValue(CustomTabControlProperty);
			set => SetValue(CustomTabControlProperty, value);
		}

		public string CustomTabId
		{
			get => GetValue(CustomTabIdProperty);
			set => SetValue(CustomTabIdProperty, value);
		}

		public Shared.Services.Tab Tab { get; set; }
		public virtual TabState GetTabState() => new TabState(Tab);

		public event Func<bool>? GoBack;

		public virtual bool BackRequested()
		{
			if (GoBack == null)
				return false;
			return GoBack.Invoke();
		}

		public virtual void Dispose()
		{
		}

		public static readonly StyledProperty<object?> CustomTabControlProperty = AvaloniaProperty.Register<ModernTab, object?>("CustomTabControl");
		public static readonly StyledProperty<string> CustomTabIdProperty = AvaloniaProperty.Register<ModernTab, string>("CustomTabId");
	}
}
