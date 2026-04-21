namespace LRReader.Avalonia.Views.Controls;

public partial class ModernExpander : Expander
{
	protected override Type StyleKeyOverride => typeof(Expander);

	public ModernExpander()
	{
		Items = [];
		InitializeComponent();
	}

	public object? Input
	{
		get => GetValue(InputProperty);
		set => SetValue(InputProperty, value);
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

	public IList<object> Items
	{
		get => GetValue(ItemsProperty);
		set => SetValue(ItemsProperty, value);
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

	public string? ToolTip
	{
		get => GetValue(ToolTipProperty);
		set => SetValue(ToolTipProperty, value);
	}

	public static readonly StyledProperty<object?> InputProperty = AvaloniaProperty.Register<ModernExpander, object?>("Input");
	public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<ModernExpander, string>("Title", "");
	public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<ModernExpander, string>("Description", "");
	public static readonly StyledProperty<IList<object>> ItemsProperty = AvaloniaProperty.Register<ModernExpander, IList<object>>("Items");
	public static readonly StyledProperty<string?> GlyphProperty = AvaloniaProperty.Register<ModernExpander, string?>("Glyph");
	public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<ModernExpander, object?>("Icon");
	public static readonly StyledProperty<string?> ToolTipProperty = AvaloniaProperty.Register<ModernExpander, string?>("ToolTip");
}
