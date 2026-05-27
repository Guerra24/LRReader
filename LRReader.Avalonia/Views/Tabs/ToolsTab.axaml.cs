using LRReader.Avalonia.Views.Controls;

namespace LRReader.Avalonia.Views.Tabs;

public partial class ToolsTab : ModernTab
{
	public ToolsTab()
	{
		InitializeComponent();
		GoBack += ContentPage.GoBack;
	}
}