using LRReader.UWP.Views.Controls;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ToolsTab : ModernTab
	{
		public ToolsTab()
		{
			this.InitializeComponent();
			GoBack += () => ContentPage.GoBack();
		}
	}
}
