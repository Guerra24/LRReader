using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinRT;

namespace LRReader.UWP.Views.Controls
{
	public partial class ModernBasePage : Page
	{

		protected ModernPageTabWrapper Wrapper = null!;

		//private bool _navigating;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			if (e.Parameter is ModernPageTabWrapper wrapper && Wrapper == null)
				Wrapper = wrapper;
		}

		[DynamicWindowsRuntimeCast(typeof(Frame))]
		protected void PageButton_Click(object sender, RoutedEventArgs e)
		{
			/*if (_navigating)
				return;
			_navigating = true;*/
			if (Parent == null)
				return;
			Wrapper.ModernPageTab.Navigate((ModernPageTabItem)((ModernInput)sender).Tag, (int)((Frame)Parent).Tag);
		}
	}
}
