using CommunityToolkit.WinUI.Behaviors;
using LRReader.Shared.Services;

namespace LRReader.UWP.Views.Controls
{
	public partial class ModernTabView : TabView
	{

		private const int SPLIT = 888;

		private Button? TogglePaneButton;
		private SplitView? SplitView;
		/*private ColumnDefinition? FooterColumn;
		private ColumnDefinition? ToolsColumn;
		private ColumnDefinition? BlankColumn;
		private ColumnDefinition? SpacerColumn;
		private ColumnDefinition? ExtraColumn;
		private ContentPresenter? ToolsContentPresenter;*/
		//private ContentPresenter? RightContentPresenter;
		public StackedNotificationsBehavior Notifications { get; private set; } = null!;

		public ModernTabView()
		{
			Loaded += ModernTabView_Loaded;
			Unloaded += ModernTabView_Unloaded;
			SizeChanged += ModernTabView_SizeChanged;
		}

		[DynamicWindowsRuntimeCast(typeof(Button))]
		[DynamicWindowsRuntimeCast(typeof(SplitView))]
		[DynamicWindowsRuntimeCast(typeof(ColumnDefinition))]
		[DynamicWindowsRuntimeCast(typeof(ContentPresenter))]
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			TogglePaneButton = GetTemplateChild("TogglePaneButton") as Button;
			SplitView = GetTemplateChild("SplitView") as SplitView;
			/*FooterColumn = GetTemplateChild("FooterColumn") as ColumnDefinition;
			ToolsColumn = GetTemplateChild("ToolsColumn") as ColumnDefinition;
			BlankColumn = GetTemplateChild("BlankColumn") as ColumnDefinition;
			SpacerColumn = GetTemplateChild("SpacerColumn") as ColumnDefinition;
			ExtraColumn = GetTemplateChild("ExtraColumn") as ColumnDefinition;
			ToolsContentPresenter = GetTemplateChild("ToolsContentPresenter") as ContentPresenter;*/
			//RightContentPresenter = GetTemplateChild("RightContentPresenter") as ContentPresenter;
			Notifications = (StackedNotificationsBehavior)GetTemplateChild("Notifications");
		}

		[GeneratedDependencyProperty]
		public partial UIElement? FakeTabStripFooter { get; set; }
		[GeneratedDependencyProperty]
		public partial UIElement? TabTools { get; set; }

		[GeneratedDependencyProperty]
		public partial UIElement? ExtraFooter { get; set; }

		[GeneratedDependencyProperty(DefaultValue = true)]
		public partial bool IsPaneOpen { get; set; }

		private void PaneToggle_Click(object sender, RoutedEventArgs e)
		{
			IsPaneOpen = !IsPaneOpen;
		}

		private void ModernTabView_Loaded(object sender, RoutedEventArgs e)
		{
			TogglePaneButton?.Click += PaneToggle_Click;
			SplitView?.PaneOpening += SplitView_PaneOpening;
			SplitView?.PaneClosed += SplitView_PaneClosed;

			if (ActualWidth < SPLIT)
				IsPaneOpen = false;

			//ToolsContentPresenter?.SizeChanged += ToolsContentPresenter_SizeChanged;
			//RightContentPresenter?.Width = FooterColumn!.ActualWidth + BlankColumn!.MinWidth + ToolsColumn!.ActualWidth + SpacerColumn!.ActualWidth + ExtraColumn!.ActualWidth;
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
			TogglePaneButton?.Click -= PaneToggle_Click;
			SplitView?.PaneOpening -= SplitView_PaneOpening;
			SplitView?.PaneClosed -= SplitView_PaneClosed;

			//ToolsContentPresenter?.SizeChanged -= ToolsContentPresenter_SizeChanged;
		}

		private void ModernTabView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!Service.Settings.UseVerticalTabs)
				return;

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

		/*private void ToolsContentPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			RightContentPresenter!.Width = FooterColumn!.ActualWidth + BlankColumn!.MinWidth + ToolsColumn!.ActualWidth + SpacerColumn!.ActualWidth + ExtraColumn!.ActualWidth;
		}*/
	}

}
