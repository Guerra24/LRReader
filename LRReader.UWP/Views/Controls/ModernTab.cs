#nullable enable
using System;
using System.Threading.Tasks;
using LRReader.Shared.Models;
using LRReader.Shared.Services;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Controls
{

	public delegate bool GoBackTabEvent();

	public class ModernTab : TabViewItem, ICustomTab
	{
		//private bool _open;

		private Flyout? Flyout;

		public ModernTab()
		{
			//this.DefaultStyleKey = typeof(ModernTab);
			if (Service.Settings.UseVerticalTabs)
			{
				Template = (ControlTemplate)App.Current.Resources["MicaVerticalTabViewItemTemplate"];
			}
			else
			{
				Template = (ControlTemplate)App.Current.Resources["MicaTabViewItemTemplate"];
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

		public object CustomTabControl
		{
			get => GetValue(CustomTabControlProperty);
			set => SetValue(CustomTabControlProperty, value);
		}

		public string CustomTabId
		{
			get => (string)GetValue(CustomTabIdProperty);
			set => SetValue(CustomTabIdProperty, value);
		}

		public RenderTargetBitmap Thumbnail
		{
			get => (RenderTargetBitmap)GetValue(ThumbnailProperty);
			set => SetValue(ThumbnailProperty, value);
		}

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

		public static readonly DependencyProperty CustomTabControlProperty = DependencyProperty.Register("CustomTabControl", typeof(object), typeof(ModernTab), new PropertyMetadata(null));
		public static readonly DependencyProperty CustomTabIdProperty = DependencyProperty.Register("CustomTabId", typeof(string), typeof(ModernTab), new PropertyMetadata(""));
		public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register("Thumbnail", typeof(RenderTargetBitmap), typeof(ModernTab), new PropertyMetadata(null));
	}
}
