using System.Diagnostics.CodeAnalysis;

namespace LRReader.Avalonia.Views.Controls;

public partial class ModernPageTab : UserControl, IDisposable
{

	public string? Title
	{
		get => GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public Type? Initial
	{
#pragma warning disable IL2073
		get => GetValue(InitialProperty);
#pragma warning restore IL2073
		set => SetValue(InitialProperty, value);
	}

	private void UserControl_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
	{
		if (Design.IsDesignMode)
			return;

		if (_loaded)
			return;
		_loaded = true;
		if (Initial != null)
			Navigate(new ModernPageTabItem { Title = Title!, Page = Initial }, 0);
	}

	public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<ModernPageTab, string?>("Title");
	public static readonly StyledProperty<Type?> InitialProperty = AvaloniaProperty.Register<ModernPageTab, Type?>("Initial");
}
