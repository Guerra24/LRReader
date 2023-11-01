using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LRReader.UWP.Views.Controls
{
	public class ModernBasePage : Page
	{

		protected ModernPageTabWrapper Wrapper = null!;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.Parameter is ModernPageTabWrapper wrapper && Wrapper == null)
				Wrapper = wrapper;
		}

		protected void PageButton_Click(object sender, RoutedEventArgs e)
		{
			Wrapper.ModernPageTab.Navigate((ModernPageTabItem)((ModernInput)sender).Tag, (int)((Frame)Parent).Tag);
		}
	}
}
