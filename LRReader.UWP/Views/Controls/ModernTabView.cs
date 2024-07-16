#nullable enable
using CommunityToolkit.WinUI.Behaviors;
using LRReader.Shared.Services;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Controls
{
	public class ModernTabView : TabView
	{

		private const int SPLIT = 888;

		private Button? TogglePaneButton;
		private SplitView? SplitView;
		public StackedNotificationsBehavior Notifications { get; private set; } = null!;

		public ModernTabView()
		{
			if (Service.Settings.UseVerticalTabs)
			{
				Loaded += ModernTabView_Loaded;
				Unloaded += ModernTabView_Unloaded;
				SizeChanged += ModernTabView_SizeChanged;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			TogglePaneButton = GetTemplateChild("TogglePaneButton") as Button;
			SplitView = GetTemplateChild("SplitView") as SplitView;
			Notifications = (StackedNotificationsBehavior)GetTemplateChild("Notifications");
		}

		public object ExtraFooter
		{
			get => GetValue(ExtraFooterProperty);
			set => SetValue(ExtraFooterProperty, value);
		}

		public bool IsPaneOpen
		{
			get => (bool)GetValue(IsPaneOpenProperty);
			set => SetValue(IsPaneOpenProperty, value);
		}

		private void PaneToggle_Click(object sender, RoutedEventArgs e)
		{
			IsPaneOpen = !IsPaneOpen;
		}

		private void ModernTabView_Loaded(object sender, RoutedEventArgs e)
		{
			TogglePaneButton!.Click += PaneToggle_Click;
			SplitView!.PaneOpening += SplitView_PaneOpening;
			SplitView!.PaneClosed += SplitView_PaneClosed;
			if (ActualWidth < SPLIT)
				IsPaneOpen = false;
		}

		private void SplitView_PaneOpening(SplitView sender, object args)
		{
			if (ActualWidth < SPLIT)
				VisualStateManager.GoToState(this, "CompactOverlay", true);
		}

		private void SplitView_PaneClosed(SplitView sender, object args)
		{
			if (ActualWidth < SPLIT)
				VisualStateManager.GoToState(this, "Normal", true);
		}

		private void ModernTabView_Unloaded(object sender, RoutedEventArgs e)
		{
			TogglePaneButton!.Click -= PaneToggle_Click;
		}

		private void ModernTabView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width >= SPLIT)
			{
				if (e.PreviousSize.Width < SPLIT)
					IsPaneOpen = true;
				VisualStateManager.GoToState(this, "Inline", true);
			}
			else
			{
				if (e.PreviousSize.Width >= SPLIT)
					IsPaneOpen = false;
			}
		}

		public static readonly DependencyProperty ExtraFooterProperty = DependencyProperty.Register("ExtraFooter", typeof(object), typeof(ModernTabView), new PropertyMetadata(null));
		public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register("IsPaneOpen", typeof(bool), typeof(ModernTabView), new PropertyMetadata(true));
	}

}
