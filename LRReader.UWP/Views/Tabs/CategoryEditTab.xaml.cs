using LRReader.Internal;
using LRReader.Shared.Models.Main;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class CategoryEditTab : CustomTab
	{
		private Category category;

		private bool loaded;

		public CategoryEditTab(Category category)
		{
			this.InitializeComponent();
			this.category = category;
			CustomTabId = "Edit_" + category.id;
			TabContent.SetCategoryInternal(category);
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
