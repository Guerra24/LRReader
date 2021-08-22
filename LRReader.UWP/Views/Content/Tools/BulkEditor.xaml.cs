using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			await Data.Load();
		}

		private void HideFlyout_Click(object sender, RoutedEventArgs e)
		{
			((sender as Button).Tag as Flyout).Hide();
		}
	}
}
