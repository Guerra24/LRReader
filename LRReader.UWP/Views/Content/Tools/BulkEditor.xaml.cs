using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Tools
{

	public sealed partial class BulkEditor : Page
	{
		private BulkEditorViewModel Data;

		public ControlFlags flags => Service.Api.ControlFlags;

		public BulkEditor()
		{
			this.InitializeComponent();
			Data = DataContext as BulkEditorViewModel;
		}

		private void HideFlyout_Click(object sender, RoutedEventArgs e)
		{
			((sender as Button).Tag as Flyout).Hide();
		}
	}
}
