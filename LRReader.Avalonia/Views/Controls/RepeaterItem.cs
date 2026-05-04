using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using LRReader.Shared.Extensions;
using System.Windows.Input;

namespace LRReader.Avalonia.Views.Controls
{

	[PseudoClasses(":pressed")]
	public partial class RepeaterItem : ContentControl
	{
		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			PseudoClasses.Set(":pressed", false);
		}

		public ICommand? Command
		{
			get => GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object? CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public BoxShadows BoxShadow
		{
			get => GetValue(BoxShadowProperty);
			set => SetValue(BoxShadowProperty, value);
		}

		public event EventHandler<ItemClickEventArgs>? Click
		{
			add => AddHandler(ClickEvent, value);
			remove => RemoveHandler(ClickEvent, value);
		}

		protected override void OnPointerEntered(PointerEventArgs e)
		{
			base.OnPointerEntered(e);
			//if (IsButton && IsEnabled)
			//VisualStateManager.GoToState(this, "PointerOver", true);
		}

		protected override void OnPointerExited(PointerEventArgs e)
		{
			base.OnPointerExited(e);
			//if (IsButton && IsEnabled)
			//VisualStateManager.GoToState(this, "Normal", true);
		}

		protected override void OnPointerPressed(PointerPressedEventArgs e)
		{
			var point = e.GetCurrentPoint(this);
			if (!point.Properties.IsLeftButtonPressed)
				return;
			base.OnPointerPressed(e);
			if (IsEnabled)
				PseudoClasses.Set(":pressed", true);
			//VisualStateManager.GoToState(this, "Pressed", true);
		}

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
			var point = e.GetCurrentPoint(this);
			if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
				return;
			base.OnPointerReleased(e);
			if (IsEnabled)
			{
				var param = new GridViewExtParameter(!e.KeyModifiers.HasFlag(KeyModifiers.Control), CommandParameter!);
				if (Command != null && Command.CanExecute(param))
					Command.Execute(param);
				RaiseEvent(new ItemClickEventArgs(ClickEvent, this, DataContext!));
				PseudoClasses.Set(":pressed", false);
				//VisualStateManager.GoToState(this, "PointerOver", true);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (IsEnabled && (e.Key == Key.Space || e.Key == Key.Enter))
				PseudoClasses.Set(":pressed", true);
			//VisualStateManager.GoToState(this, "Pressed", true);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (IsEnabled && (e.Key == Key.Space || e.Key == Key.Enter))
			{
				RaiseEvent(new ItemClickEventArgs(ClickEvent, this, DataContext!));
				PseudoClasses.Set(":pressed", false);
				//VisualStateManager.GoToState(this, "Normal", true);
			}
		}

		protected override void OnLostFocus(FocusChangedEventArgs e)
		{
			base.OnLostFocus(e);
			if (IsEnabled)
				PseudoClasses.Set(":pressed", false);
			//VisualStateManager.GoToState(this, "Normal", true);
		}

		public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<RepeaterItem, ICommand?>("Command", enableDataValidation: true);
		public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<RepeaterItem, object?>("CommandParameter");
		public static readonly StyledProperty<BoxShadows> BoxShadowProperty = AvaloniaProperty.Register<RepeaterItem, BoxShadows>("BoxShadow");
		public static readonly RoutedEvent<ItemClickEventArgs> ClickEvent = RoutedEvent.Register<RepeaterItem, ItemClickEventArgs>(nameof(Click), RoutingStrategies.Bubble);
	}

	public class ItemClickEventArgs : RoutedEventArgs
	{
		public object ClickedItem { get; set; }

		public ItemClickEventArgs(RoutedEvent? routedEvent, object? source, object clickedItem) : base(routedEvent, source)
		{
			ClickedItem = clickedItem;
		}
	}
}
