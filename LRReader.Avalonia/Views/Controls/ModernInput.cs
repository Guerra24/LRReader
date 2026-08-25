using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using LRReader.Avalonia.Extensions;
using System.Windows.Input;

namespace LRReader.Avalonia.Views.Controls;

[PseudoClasses(":isbutton", ":pressed")]
public sealed partial class ModernInput : ContentControl
{
	public ModernInput()
	{
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		this.SetRepositionAnimation();
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);

		PseudoClasses.Set(":isbutton", IsButton);
		PseudoClasses.Set(":pressed", false);
	}

	public string Title
	{
		get => GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string Description
	{
		get => GetValue(DescriptionProperty);
		set => SetValue(DescriptionProperty, value);
	}

	public HorizontalAlignment InputHorizontalAlignment
	{
		get => GetValue(InputHorizontalAlignmentProperty);
		set => SetValue(InputHorizontalAlignmentProperty, value);
	}

	public Thickness TextMargin
	{
		get => GetValue(TextMarginProperty);
		set => SetValue(TextMarginProperty, value);
	}

	public string? Glyph
	{
		get => GetValue(GlyphProperty);
		set => SetValue(GlyphProperty, value);
	}

	public object? Icon
	{
		get => GetValue(IconProperty);
		set => SetValue(IconProperty, value);
	}

	public bool IsButton
	{
		get => GetValue(IsButtonProperty);
		set => SetValue(IsButtonProperty, IsTabStop = value);
	}

	public string? RightGlyph
	{
		get => GetValue(RightGlyphProperty);
		set => SetValue(RightGlyphProperty, value);
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

	public GridLength RightColumnWidth
	{
		get => GetValue(RightColumnWidthProperty);
		set => SetValue(RightColumnWidthProperty, value);
	}

	public event EventHandler<RoutedEventArgs>? Click
	{
		add => AddHandler(ClickEvent, value);
		remove => RemoveHandler(ClickEvent, value);
	}

	public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<ModernInput, string>("Title", "");
	public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<ModernInput, string>("Description", "");
	public static readonly StyledProperty<HorizontalAlignment> InputHorizontalAlignmentProperty = AvaloniaProperty.Register<ModernInput, HorizontalAlignment>("InputHorizontalAlignment", HorizontalAlignment.Right);
	public static readonly StyledProperty<Thickness> TextMarginProperty = AvaloniaProperty.Register<ModernInput, Thickness>("TextMargin", new Thickness(0));
	public static readonly StyledProperty<string?> GlyphProperty = AvaloniaProperty.Register<ModernInput, string?>("Glyph");
	public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<ModernInput, object?>("Icon");
	public static readonly StyledProperty<bool> IsButtonProperty = AvaloniaProperty.Register<ModernInput, bool>("IsButton");
	public static readonly StyledProperty<string?> RightGlyphProperty = AvaloniaProperty.Register<ModernInput, string?>("RightGlyph");
	public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<ModernInput, ICommand?>("Command", enableDataValidation: true);
	public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<ModernInput, object?>("CommandParameter");
	public static readonly StyledProperty<GridLength> RightColumnWidthProperty = AvaloniaProperty.Register<ModernInput, GridLength>("RightColumnWidth", GridLength.Auto);
	public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = RoutedEvent.Register<ModernInput, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);
}
