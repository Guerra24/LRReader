using CommunityToolkit.WinUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace LRReader.UWP.Views.Controls;

public partial class CustomCheckboxListViewItemPresenter : ListViewItemPresenter
{

	private static Thickness margin = new(0, 3, 3, 0);
	private static CornerRadius cornerRadius = new(3);

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		var checkbox = this.FindDescendant<Border>(border => border.Height == 20);
		checkbox?.Margin = margin;
		checkbox?.CornerRadius = cornerRadius;
	}
}
