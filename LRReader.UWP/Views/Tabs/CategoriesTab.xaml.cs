using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class CategoriesTab : ModernTab
	{
		public CategoriesTab()
		{
			this.InitializeComponent();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			TabContent.Refresh();
		}
	}
}
