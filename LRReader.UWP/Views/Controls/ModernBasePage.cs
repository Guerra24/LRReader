using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Controls
{
	public class ModernBasePage : Page
	{

		protected ModernPageTab Page = null!;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.Parameter is ModernPageTab page && Page == null)
				Page = page;
		}

		protected void PageButton_Click(object sender, RoutedEventArgs e)
		{
			Page.Navigate((ModernPageTabItem)((ModernInput)sender).Tag);
		}
	}
}
