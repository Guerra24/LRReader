using CommunityToolkit.WinUI;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using WinRT;

namespace LRReader.UWP.Views.Controls
{

	public delegate bool GoBackTabEvent();

	public partial class ModernTab : TabViewItem, ICustomTab
	{
		//private bool _open;

		private Flyout? Flyout;

		public ModernTab()
		{
			//this.DefaultStyleKey = typeof(ModernTab);
			if (Service.Settings.UseVerticalTabs)
			{
				Template = Application.Current.Resources["MicaVerticalTabViewItemTemplate"].As<ControlTemplate>();
			}
			else
			{
				Template = Application.Current.Resources["MicaTabViewItemTemplate"].As<ControlTemplate>();
			}
			//Thumbnail = new();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Flyout = (Flyout)GetTemplateChild("Flyout");
		}
		/*
		protected override async void OnPointerEntered(PointerRoutedEventArgs e)
		{
			base.OnPointerEntered(e);
			if (Flyout != null)
			{
				_open = true;
				await Task.Delay(TimeSpan.FromMilliseconds(Service.Platform.HoverTime));
				if (_open && !Flyout.IsOpen)
				{
					_open = false;
					if (IsSelected)
						await Thumbnail.RenderAsync((UIElement)Content);
					Flyout.ShowAt(this, new FlyoutShowOptions
					{
						Placement = FlyoutPlacementMode.Right,
						ShowMode = FlyoutShowMode.Transient
					});
				}
			}
		}

		protected override void OnPointerExited(PointerRoutedEventArgs e)
		{
			base.OnPointerExited(e);
			if (_open)
				_open = false;
		}*/

		[GeneratedDependencyProperty]
		public partial object? CustomTabControl { get; set; }

		[GeneratedDependencyProperty(DefaultValue = "")]
		public partial string CustomTabId { get; set; }

		[GeneratedDependencyProperty]
		public partial RenderTargetBitmap? Thumbnail { get; set; }

		public Tab Tab { get; set; }

		public virtual TabState GetTabState() => new TabState(Tab);

		public event GoBackTabEvent? GoBack;

		public virtual void Dispose()
		{
		}

		public virtual bool BackRequested()
		{
			if (GoBack == null)
				return false;
			return GoBack.Invoke();
		}
	}
}
