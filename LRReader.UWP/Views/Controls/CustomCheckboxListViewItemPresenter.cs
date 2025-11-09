using CommunityToolkit.WinUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace LRReader.UWP.Views.Controls;

public partial class CustomCheckboxListViewItemPresenter : ListViewItemPresenter
{

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		var checkbox = this.FindDescendant<Border>(border => border.Height == 20);
		checkbox?.Margin = new Thickness(0, 3, 3, 0);
		checkbox?.CornerRadius = new CornerRadius(3);
	}
}
