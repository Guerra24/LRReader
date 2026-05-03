using Avalonia.Interactivity;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using System.Windows.Input;

namespace LRReader.Avalonia.Views.Controls;

public partial class ArchiveTags : UserControl
{
	public ArchiveTags()
	{
		ItemsSource = [];
		InitializeComponent();
	}

	private void Tags_CopyTag(object? sender, RoutedEventArgs e)
	{
		Service.Platform.CopyToClipboard((string)((MenuItem)sender!).Tag!);
	}

	public ICommand? ItemClickCommand
	{
		get => GetValue(ItemClickCommandProperty);
		set => SetValue(ItemClickCommandProperty, value);
	}

	public IList<ArchiveTagsGroup> ItemsSource
	{
		get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	public double MaxTagsWidth
	{
		get => GetValue(MaxTagsWidthProperty);
		set => SetValue(MaxTagsWidthProperty, value);
	}

	private void TagsList_Loaded(object? sender, RoutedEventArgs e)
	{
		var grid = sender as ItemsRepeater;
		if (grid?.Tag == null)
		{
			grid!.MaxWidth = MaxTagsWidth;
			grid.Tag = "The absolute state of UWP Bindings";
		}
	}

	public static readonly StyledProperty<ICommand?> ItemClickCommandProperty = AvaloniaProperty.Register<ArchiveTags, ICommand?>("ItemClickCommand", enableDataValidation: true);
	public static readonly StyledProperty<IList<ArchiveTagsGroup>> ItemsSourceProperty = AvaloniaProperty.Register<ArchiveTags, IList<ArchiveTagsGroup>>("ItemsSource");
	public static readonly StyledProperty<double> MaxTagsWidthProperty = AvaloniaProperty.Register<ArchiveTags, double>("MaxTagsWidth", double.MaxValue);
}