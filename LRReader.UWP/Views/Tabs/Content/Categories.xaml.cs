using LRReader.Shared.Models.Main;
using LRReader.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class Categories : UserControl
	{
		public CategoriesViewModel Data;

		private bool loaded;

		public Categories()
		{
			this.InitializeComponent();
			Data = DataContext as CategoriesViewModel;
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			await Data.Refresh();
		}

		private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
		{
			using (var deferral = args.GetDeferral())
			{
				await Data.Refresh();
			}
		}

		private async void Refresh_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => await Data.Refresh();

	}

	public class CategoryTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StaticTemplate { get; set; }
		public DataTemplate DynamicTemplate { get; set; }
		public DataTemplate AddNewTemplate { get; set; }
		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is AddNewCategory)
				return AddNewTemplate;
			if (item is Category)
				return StaticTemplate;
			return base.SelectTemplateCore(item);
		}
		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return SelectTemplateCore(item);
		}
	}

}
