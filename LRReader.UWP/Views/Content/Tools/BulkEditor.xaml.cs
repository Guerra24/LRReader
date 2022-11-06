using LRReader.Shared.ViewModels.Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Tools
{

	public sealed partial class BulkEditor : Page
	{
		private BulkEditorViewModel Data;

		public BulkEditor()
		{
			this.InitializeComponent();
			Data = (BulkEditorViewModel)DataContext;
		}

		private void HideFlyout_Click(object sender, RoutedEventArgs e) => ((Flyout)((Button)sender).Tag).Hide();
	}
}
