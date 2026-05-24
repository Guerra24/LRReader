using Avalonia.Interactivity;
using LRReader.Avalonia.Views.Controls;

namespace LRReader.Avalonia.Views.Tabs;

public partial class BookmarksTab : ModernTab
{
	public BookmarksTab()
	{
		InitializeComponent();
	}

	private void ExportButton_Click(object sender, RoutedEventArgs e)
	{
		TabContent.ExportBookmarks();
	}

	private void ImportButton_Click(object sender, RoutedEventArgs e)
	{
		TabContent.ImportBookmarks();
	}
}