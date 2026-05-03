using LRReader.Shared.Extensions;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using Windows.UI.Core;

namespace LRReader.UWP.Views.Controls
{
	public sealed partial class ArchiveTags : UserControl
	{
		public ArchiveTags()
		{
			this.InitializeComponent();
		}

		[DynamicWindowsRuntimeCast(typeof(MenuFlyoutItem))]
		private void Tags_CopyTag(object sender, RoutedEventArgs e)
		{
			Service.Platform.CopyToClipboard((string)((MenuFlyoutItem)sender).Tag);
		}

		private void Tags_ItemClick(object sender, ItemClickEventArgs e)
		{
			var param = new GridViewExtParameter((CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down, e.ClickedItem);
			if (ItemClickCommand?.CanExecute(param) ?? false)
				ItemClickCommand.Execute(param);
		}

		[GeneratedDependencyProperty]
		public partial ICommand? ItemClickCommand { get; set; }

		[GeneratedDependencyProperty(DefaultValueCallback = "GetDefaultItemSource")]
		public partial IList<ArchiveTagsGroup> ItemsSource { get; set; }

		private static IList<ArchiveTagsGroup> GetDefaultItemSource() => new List<ArchiveTagsGroup>();

		[GeneratedDependencyProperty(DefaultValue = double.MaxValue)]
		public partial double MaxTagsWidth { get; set; }

		[DynamicWindowsRuntimeCast(typeof(GridView))]
		private void TagsList_Loaded(object sender, RoutedEventArgs e)
		{
			var grid = sender as GridView;
			if (grid?.Tag == null)
			{
				grid!.MaxWidth = MaxTagsWidth;
				grid.Tag = "The absolute state of UWP Bindings";
			}
		}
	}
}
