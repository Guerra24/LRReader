#nullable enable
using CommunityToolkit.WinUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace LRReader.UWP.Views.Controls
{
	// Why
	public class NoBorderListViewItemPresenter : ListViewItemPresenter
	{
		public NoBorderListViewItemPresenter()
		{
			Loaded += NoBorderListViewItemPresenter_Loaded;
		}

		private void NoBorderListViewItemPresenter_Loaded(object sender, RoutedEventArgs e)
		{
			var border = this.FindDescendant<Border>();
			border!.Margin = new Thickness(0);
			Loaded -= NoBorderListViewItemPresenter_Loaded;
		}

	}
}
