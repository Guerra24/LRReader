using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class CategoryEditTab : ModernTab
	{
		private Category category;

		private bool loaded;

		public CategoryEditTab(Category category)
		{
			this.InitializeComponent();
			this.category = category;
			CustomTabId = "Edit_" + category.id;
		}

		private void TabViewItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			TabContent.LoadCategory(category);
		}

		private async void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			await TabContent.Refresh();
		}
	}
}
