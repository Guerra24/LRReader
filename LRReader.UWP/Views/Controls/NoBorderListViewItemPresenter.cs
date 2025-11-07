using CommunityToolkit.WinUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace LRReader.UWP.Views.Controls;

// Why
public partial class NoBorderListViewItemPresenter : ListViewItemPresenter
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

public partial class CustomCheckboxListViewItemPresenter : ListViewItemPresenter
{

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		var checkbox = this.FindDescendant<Border>(border => border.Height == 20);
		checkbox?.Margin = new Thickness(0, 3, 3, 0);
	}
}
