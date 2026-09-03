using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using LRReader.Shared.Services;

namespace LRReader.Avalonia.Views.Controls
{
	public partial class ModernTabView : FATabView
	{

		private const int SPLIT = 888;

		private Button? TogglePaneButton;
		private SplitView? SplitView;
		private ContentPresenter? RightContentPresenter;
		//public StackedNotificationsBehavior Notifications { get; private set; } = null!;

		public ModernTabView()
		{
			Loaded += ModernTabView_Loaded;
			Unloaded += ModernTabView_Unloaded;
			SizeChanged += ModernTabView_SizeChanged;
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			TogglePaneButton = e.NameScope.Find<Button>("TogglePaneButton");
			SplitView = e.NameScope.Find<SplitView>("SplitView");
			RightContentPresenter = e.NameScope.Find<ContentPresenter>("RightContentPresenter");
			//Notifications = (StackedNotificationsBehavior)GetTemplateChild("Notifications");
		}

		public object? FakeTabStripFooter
		{
			get => GetValue(FakeTabStripFooterProperty);
			set => SetValue(FakeTabStripFooterProperty, value);
		}

		public object? TabTools
		{
			get => GetValue(TabToolsProperty);
			set => SetValue(TabToolsProperty, value);
		}

		public object? ExtraFooter
		{
			get => GetValue(ExtraFooterProperty);
			set => SetValue(ExtraFooterProperty, value);
		}

		public bool IsPaneOpen
		{
			get => GetValue(IsPaneOpenProperty);
			set => SetValue(IsPaneOpenProperty, value);
		}

		private void PaneToggle_Click(object? sender, RoutedEventArgs e)
		{
			IsPaneOpen = !IsPaneOpen;
		}

		private void ModernTabView_Loaded(object? sender, RoutedEventArgs e)
		{
			TogglePaneButton?.Click += PaneToggle_Click;
			SplitView?.PaneOpening += SplitView_PaneOpening;
			SplitView?.PaneClosed += SplitView_PaneClosed;

			if (Bounds.Width < SPLIT)
				IsPaneOpen = false;

			RightContentPresenter?.Width = 305; // This should be dynamic but ALAS here we are
		}

		private void SplitView_PaneOpening(object? sender, CancelRoutedEventArgs args)
		{
			//if (Bounds.Width < SPLIT)
			//	VisualStateManager.GoToState(this, "CompactOverlay", true);
		}

		private void SplitView_PaneClosed(object? sender, RoutedEventArgs args)
		{
			//if (Bounds.Width < SPLIT)
			//	VisualStateManager.GoToState(this, "Normal", true);
		}

		private void ModernTabView_Unloaded(object? sender, RoutedEventArgs e)
		{
			TogglePaneButton?.Click -= PaneToggle_Click;
		}

		private void ModernTabView_SizeChanged(object? sender, SizeChangedEventArgs e)
		{
			if (!Service.Settings.UseVerticalTabs)
				return;

			/*if (e.NewSize.Width >= SPLIT)
			{
				if (e.PreviousSize.Width < SPLIT)
					IsPaneOpen = true;
				VisualStateManager.GoToState(this, "Inline", true);
			}
			else
			{
				if (e.PreviousSize.Width >= SPLIT)
					IsPaneOpen = false;
			}*/
		}

		public static readonly StyledProperty<object?> FakeTabStripFooterProperty = AvaloniaProperty.Register<ModernTabView, object?>("FakeTabStripFooter");
		public static readonly StyledProperty<object?> TabToolsProperty = AvaloniaProperty.Register<ModernTabView, object?>("TabTools");
		public static readonly StyledProperty<object?> ExtraFooterProperty = AvaloniaProperty.Register<ModernTabView, object?>("ExtraFooter");
		public static readonly StyledProperty<bool> IsPaneOpenProperty = AvaloniaProperty.Register<ModernTabView, bool>("IsPaneOpen", true);

	}
}
